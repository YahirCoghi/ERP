using System;
using System.Collections.Generic;
using MiniERP.Domain.Entities.Accounting;

namespace MiniERP.Application.DTOs;

public record AccountDto(
    int Id,
    string Code,
    string Name,
    AccountType Type,
    int? ParentAccountId,
    bool IsPosting);

public record CreateAccountRequest(
    string Code,
    string Name,
    AccountType Type,
    int? ParentAccountId,
    bool IsPosting);

public record JournalEntryLineDto(
    int AccountId,
    decimal Debit,
    decimal Credit,
    string? Description);

public record JournalEntryDto(
    int Id,
    string EntryNumber,
    DateTime EntryDate,
    string? Description,
    JournalEntryStatus Status,
    int FiscalPeriodId,
    IReadOnlyList<JournalEntryLineDto> Lines);

public record CreateJournalEntryRequest(
    string EntryNumber,
    DateTime EntryDate,
    string? Description,
    int FiscalPeriodId,
    IReadOnlyList<JournalEntryLineDto> Lines);

public record FiscalPeriodDto(
    int Id,
    string Code,
    DateTime StartDate,
    DateTime EndDate,
    bool IsClosed);

public record CreateFiscalPeriodRequest(
    string Code,
    DateTime StartDate,
    DateTime EndDate);
