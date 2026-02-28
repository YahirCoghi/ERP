using System;
using MiniERP.Domain.Entities.Common;

namespace MiniERP.Domain.Entities.Accounting;

public class FiscalPeriod : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsClosed { get; set; }
}
