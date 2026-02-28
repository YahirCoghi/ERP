using System.Threading.Tasks;

namespace MiniERP.Application.Contracts;

public interface IElectronicInvoiceSigner
{
    Task<string> SignAsync(string unsignedXml);
}
