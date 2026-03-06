using System;

namespace MiniERP.Application.DTOs;

public record IssueElectronicInvoiceRequest(
    int InvoiceId,
    string DocumentType = "01",
    bool ForceRebuildXml = false);

public record ElectronicInvoiceStatusDto(
    int Id,
    int InvoiceId,
    string DocumentType,
    string Key,
    string Consecutive,
    string Status,
    string? HaciendaStatus,
    string? HaciendaResponse,
    DateTime CreatedAt);
