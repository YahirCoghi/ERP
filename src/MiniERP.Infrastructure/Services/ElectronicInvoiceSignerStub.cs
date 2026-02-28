using System.Threading.Tasks;
using MiniERP.Application.Contracts;

namespace MiniERP.Infrastructure.Services;

public class ElectronicInvoiceSignerStub : IElectronicInvoiceSigner
{
    public Task<string> SignAsync(string unsignedXml)
    {
        // Stub: returns the unsigned XML until a real XAdES-EPES signer is plugged in.
        return Task.FromResult(unsignedXml);
    }
}
