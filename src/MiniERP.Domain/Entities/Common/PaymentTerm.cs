namespace MiniERP.Domain.Entities.Common;

public class PaymentTerm : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Days { get; set; }
    public PaymentTermType AppliesTo { get; set; } = PaymentTermType.Both;
}
