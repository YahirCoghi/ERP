using System;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MiniERP.Application.Contracts;
using MiniERP.Application.DTOs;
using MiniERP.Application.Services;
using MiniERP.Domain.Entities.Sales;
using MiniERP.Infrastructure.Data;

namespace MiniERP.Infrastructure.Services;

public class ElectronicInvoiceService : IElectronicInvoiceService
{
    private readonly ApplicationDbContext _context;
    private readonly IElectronicInvoiceSigner _signer;
    private readonly IHaciendaClient _haciendaClient;
    private readonly ElectronicInvoiceXmlBuilder _xmlBuilder;
    private readonly IElectronicDocumentValidator _validator;
    private readonly HaciendaOptions _options;

    public ElectronicInvoiceService(
        ApplicationDbContext context,
        IElectronicInvoiceSigner signer,
        IHaciendaClient haciendaClient,
        ElectronicInvoiceXmlBuilder xmlBuilder,
        IElectronicDocumentValidator validator,
        Microsoft.Extensions.Options.IOptions<HaciendaOptions> options)
    {
        _context = context;
        _signer = signer;
        _haciendaClient = haciendaClient;
        _xmlBuilder = xmlBuilder;
        _validator = validator;
        _options = options.Value;
    }

    public async Task<ElectronicInvoiceStatusDto> IssueAsync(IssueElectronicInvoiceRequest request)
    {
        var invoice = await _context.Invoices
            .Include(i => i.Customer)
            .Include(i => i.Lines)
            .ThenInclude(l => l.Product)
            .FirstOrDefaultAsync(i => i.Id == request.InvoiceId);

        if (invoice == null)
        {
            throw new InvalidOperationException("Invoice not found.");
        }

        var company = await _context.CompanyProfiles.FirstOrDefaultAsync();
        if (company == null)
        {
            throw new InvalidOperationException("Company profile not configured.");
        }

        var existing = await _context.ElectronicInvoices
            .FirstOrDefaultAsync(e => e.InvoiceId == request.InvoiceId);

        if (existing != null && !request.ForceRebuildXml)
        {
            return Map(existing);
        }

        if (request.DocumentType != "01" && request.DocumentType != "03")
        {
            throw new InvalidOperationException("Only factura (01) and nota credito (03) are supported for now.");
        }

        var issueDate = invoice.InvoiceDate == default ? DateTime.UtcNow : invoice.InvoiceDate;
        ValidateSequenceCodes(company.BranchCode, company.TerminalCode, request.DocumentType);
        var sequence = await NextSequenceAsync(request.DocumentType, company.BranchCode, company.TerminalCode);
        var consecutive = $"{company.BranchCode}{company.TerminalCode}{request.DocumentType}{sequence:0000000000}";
        var key = ElectronicDocumentKeyGenerator.GenerateKey(issueDate, company.IdentificationNumber, consecutive);
        var unsignedXml = request.DocumentType == "03"
            ? _xmlBuilder.BuildNotaCreditoXml(invoice, company, issueDate, key, consecutive, _options)
            : _xmlBuilder.BuildFacturaXml(invoice, company, issueDate, key, consecutive, _options);

        var validation = _validator.Validate(request.DocumentType, unsignedXml);
        if (!validation.IsValid)
        {
            throw new InvalidOperationException($"XML validation failed: {string.Join("; ", validation.Errors)}");
        }
        var signedXml = await _signer.SignAsync(unsignedXml);
        var signedXmlBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(signedXml));

        var submission = await _haciendaClient.SubmitAsync(new HaciendaSubmissionRequest(
            key,
            issueDate,
            HaciendaMappings.ToIdentificationCode(company.IdentificationType),
            HaciendaMappings.NormalizeIdentification(company.IdentificationNumber),
            HaciendaMappings.ToIdentificationCode(invoice.Customer?.IdentificationType),
            HaciendaMappings.NormalizeIdentification(invoice.Customer?.IdentificationNumber),
            signedXmlBase64,
            null));

        if (existing == null)
        {
            existing = new ElectronicInvoice
            {
                InvoiceId = invoice.Id
            };
            _context.ElectronicInvoices.Add(existing);
        }

        existing.DocumentType = request.DocumentType;
        existing.Key = key;
        existing.Consecutive = consecutive;
        existing.XmlUnsigned = unsignedXml;
        existing.XmlSigned = signedXml;
        existing.Status = submission.Status;
        existing.HaciendaStatus = submission.HaciendaStatus;
        existing.HaciendaResponse = submission.HaciendaResponse ?? submission.Message;
        existing.SentAt = submission.Status == "Submitted" ? DateTime.UtcNow : null;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Map(existing);
    }

    public async Task<ElectronicInvoiceStatusDto> RefreshStatusAsync(int electronicInvoiceId)
    {
        var invoice = await _context.ElectronicInvoices.FirstOrDefaultAsync(e => e.Id == electronicInvoiceId);
        if (invoice == null)
        {
            throw new InvalidOperationException("Electronic invoice not found.");
        }

        var status = await _haciendaClient.QueryStatusAsync(invoice.Key);
        invoice.Status = status.Status;
        invoice.HaciendaStatus = status.HaciendaStatus;
        invoice.HaciendaResponse = status.HaciendaResponse ?? status.Message;
        invoice.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Map(invoice);
    }

    private async Task<int> NextSequenceAsync(string documentType, string branchCode, string terminalCode)
    {
        using var tx = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);

        var sequence = await _context.DocumentSequences
            .FirstOrDefaultAsync(s => s.DocumentType == documentType
                && s.BranchCode == branchCode
                && s.TerminalCode == terminalCode);

        if (sequence == null)
        {
            sequence = new DocumentSequence
            {
                DocumentType = documentType,
                BranchCode = branchCode,
                TerminalCode = terminalCode,
                LastNumber = 0
            };
            _context.DocumentSequences.Add(sequence);
        }

        sequence.LastNumber += 1;
        sequence.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        await tx.CommitAsync();

        return sequence.LastNumber;
    }

    private static void ValidateSequenceCodes(string branchCode, string terminalCode, string documentType)
    {
        if (string.IsNullOrWhiteSpace(branchCode) || branchCode.Length != 3)
        {
            throw new InvalidOperationException("Branch code must be 3 digits.");
        }

        if (string.IsNullOrWhiteSpace(terminalCode) || terminalCode.Length != 5)
        {
            throw new InvalidOperationException("Terminal code must be 5 digits.");
        }

        if (string.IsNullOrWhiteSpace(documentType) || documentType.Length != 2)
        {
            throw new InvalidOperationException("Document type must be 2 digits.");
        }
    }

    private static ElectronicInvoiceStatusDto Map(ElectronicInvoice entity) =>
        new(entity.Id, entity.InvoiceId, entity.DocumentType, entity.Key, entity.Consecutive, entity.Status, entity.HaciendaStatus, entity.HaciendaResponse, entity.CreatedAt);
}
