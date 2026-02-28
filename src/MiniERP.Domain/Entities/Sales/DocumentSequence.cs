using MiniERP.Domain.Entities.Common;

namespace MiniERP.Domain.Entities.Sales;

public class DocumentSequence : BaseEntity
{
    public string DocumentType { get; set; } = "01";
    public string BranchCode { get; set; } = "001";
    public string TerminalCode { get; set; } = "00001";
    public int LastNumber { get; set; }
}
