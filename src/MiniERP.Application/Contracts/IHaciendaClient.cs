using System;
using System.Threading.Tasks;

namespace MiniERP.Application.Contracts;

public record HaciendaSubmissionRequest(
    string Key,
    DateTime IssueDate,
    string EmisorTipo,
    string EmisorNumero,
    string? ReceptorTipo,
    string? ReceptorNumero,
    string SignedXmlBase64,
    string? CallbackUrl);

public record HaciendaSubmissionResult(
    string Status,
    string Message,
    string? HaciendaStatus,
    string? HaciendaResponse);

public interface IHaciendaClient
{
    Task<HaciendaSubmissionResult> SubmitAsync(HaciendaSubmissionRequest request);
    Task<HaciendaSubmissionResult> QueryStatusAsync(string key);
}
