using System.Threading.Tasks;
using MiniERP.Application.DTOs;

namespace MiniERP.Application.Services;

public interface IElectronicInvoiceService
{
    Task<ElectronicInvoiceStatusDto> IssueAsync(IssueElectronicInvoiceRequest request);
    Task<ElectronicInvoiceStatusDto> RefreshStatusAsync(int electronicInvoiceId);
}
