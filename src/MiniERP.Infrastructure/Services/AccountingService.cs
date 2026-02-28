using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MiniERP.Application.DTOs;
using MiniERP.Application.Services;
using MiniERP.Domain.Entities.Accounting;
using MiniERP.Infrastructure.Data;

namespace MiniERP.Infrastructure.Services;

public class AccountingService : IAccountingService
{
    private readonly ApplicationDbContext _context;

    public AccountingService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AccountDto> CreateAccountAsync(CreateAccountRequest request)
    {
        if (await _context.Accounts.AnyAsync(a => a.Code == request.Code))
        {
            throw new InvalidOperationException("Account code already exists.");
        }

        var account = new Account
        {
            Code = request.Code.Trim(),
            Name = request.Name.Trim(),
            Type = request.Type,
            ParentAccountId = request.ParentAccountId,
            IsPosting = request.IsPosting
        };

        _context.Accounts.Add(account);
        await _context.SaveChangesAsync();

        return MapAccount(account);
    }

    public async Task<IReadOnlyList<AccountDto>> GetAccountsAsync()
    {
        var accounts = await _context.Accounts
            .OrderBy(a => a.Code)
            .ToListAsync();

        return accounts.Select(MapAccount).ToList();
    }

    public async Task<FiscalPeriodDto> CreateFiscalPeriodAsync(CreateFiscalPeriodRequest request)
    {
        if (request.EndDate < request.StartDate)
        {
            throw new InvalidOperationException("End date must be after start date.");
        }

        if (await _context.FiscalPeriods.AnyAsync(p => p.Code == request.Code))
        {
            throw new InvalidOperationException("Fiscal period code already exists.");
        }

        var period = new FiscalPeriod
        {
            Code = request.Code.Trim(),
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            IsClosed = false
        };

        _context.FiscalPeriods.Add(period);
        await _context.SaveChangesAsync();

        return MapPeriod(period);
    }

    public async Task<IReadOnlyList<FiscalPeriodDto>> GetFiscalPeriodsAsync()
    {
        var periods = await _context.FiscalPeriods
            .OrderByDescending(p => p.StartDate)
            .ToListAsync();

        return periods.Select(MapPeriod).ToList();
    }

    public async Task CloseFiscalPeriodAsync(int fiscalPeriodId)
    {
        var period = await _context.FiscalPeriods.FirstOrDefaultAsync(p => p.Id == fiscalPeriodId);
        if (period == null)
        {
            throw new InvalidOperationException("Fiscal period not found.");
        }

        var hasDrafts = await _context.JournalEntries.AnyAsync(e =>
            e.FiscalPeriodId == fiscalPeriodId && e.Status == JournalEntryStatus.Draft);

        if (hasDrafts)
        {
            throw new InvalidOperationException("Cannot close period with draft journal entries.");
        }

        period.IsClosed = true;
        period.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task<JournalEntryDto> CreateJournalEntryAsync(CreateJournalEntryRequest request)
    {
        var period = await _context.FiscalPeriods.FirstOrDefaultAsync(p => p.Id == request.FiscalPeriodId);
        if (period == null)
        {
            throw new InvalidOperationException("Fiscal period not found.");
        }

        if (period.IsClosed)
        {
            throw new InvalidOperationException("Fiscal period is closed.");
        }

        ValidateLines(request.Lines);

        var entry = new JournalEntry
        {
            EntryNumber = request.EntryNumber.Trim(),
            EntryDate = request.EntryDate,
            Description = request.Description,
            FiscalPeriodId = request.FiscalPeriodId,
            Status = JournalEntryStatus.Draft,
            Lines = request.Lines.Select(line => new JournalEntryLine
            {
                AccountId = line.AccountId,
                Debit = line.Debit,
                Credit = line.Credit,
                Description = line.Description
            }).ToList()
        };

        _context.JournalEntries.Add(entry);
        await _context.SaveChangesAsync();

        return MapEntry(entry);
    }

    public async Task<JournalEntryDto> PostJournalEntryAsync(int journalEntryId)
    {
        var entry = await _context.JournalEntries
            .Include(e => e.Lines)
            .FirstOrDefaultAsync(e => e.Id == journalEntryId);

        if (entry == null)
        {
            throw new InvalidOperationException("Journal entry not found.");
        }

        if (entry.Status != JournalEntryStatus.Draft)
        {
            throw new InvalidOperationException("Only draft entries can be posted.");
        }

        ValidateLines(entry.Lines.Select(l => new JournalEntryLineDto(l.AccountId, l.Debit, l.Credit, l.Description)).ToList());

        entry.Status = JournalEntryStatus.Posted;
        entry.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return MapEntry(entry);
    }

    public async Task<IReadOnlyList<JournalEntryDto>> GetJournalEntriesAsync()
    {
        var entries = await _context.JournalEntries
            .Include(e => e.Lines)
            .OrderByDescending(e => e.EntryDate)
            .ToListAsync();

        return entries.Select(MapEntry).ToList();
    }

    private static void ValidateLines(IReadOnlyList<JournalEntryLineDto> lines)
    {
        if (lines.Count == 0)
        {
            throw new InvalidOperationException("Journal entry must have at least one line.");
        }

        var totalDebit = lines.Sum(l => l.Debit);
        var totalCredit = lines.Sum(l => l.Credit);

        if (totalDebit <= 0 || totalCredit <= 0 || totalDebit != totalCredit)
        {
            throw new InvalidOperationException("Journal entry is not balanced.");
        }
    }

    private static AccountDto MapAccount(Account account) =>
        new(account.Id, account.Code, account.Name, account.Type, account.ParentAccountId, account.IsPosting);

    private static FiscalPeriodDto MapPeriod(FiscalPeriod period) =>
        new(period.Id, period.Code, period.StartDate, period.EndDate, period.IsClosed);

    private static JournalEntryDto MapEntry(JournalEntry entry) =>
        new(
            entry.Id,
            entry.EntryNumber,
            entry.EntryDate,
            entry.Description,
            entry.Status,
            entry.FiscalPeriodId,
            entry.Lines.Select(l => new JournalEntryLineDto(l.AccountId, l.Debit, l.Credit, l.Description)).ToList()
        );
}
