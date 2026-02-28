using System.Collections.Generic;
using System.Threading.Tasks;
using MiniERP.Application.DTOs;

namespace MiniERP.Application.Services;

public interface IAccountingService
{
    Task<AccountDto> CreateAccountAsync(CreateAccountRequest request);
    Task<IReadOnlyList<AccountDto>> GetAccountsAsync();
    Task<JournalEntryDto> CreateJournalEntryAsync(CreateJournalEntryRequest request);
    Task<JournalEntryDto> PostJournalEntryAsync(int journalEntryId);
    Task<IReadOnlyList<JournalEntryDto>> GetJournalEntriesAsync();
    Task<FiscalPeriodDto> CreateFiscalPeriodAsync(CreateFiscalPeriodRequest request);
    Task<IReadOnlyList<FiscalPeriodDto>> GetFiscalPeriodsAsync();
    Task CloseFiscalPeriodAsync(int fiscalPeriodId);
}
