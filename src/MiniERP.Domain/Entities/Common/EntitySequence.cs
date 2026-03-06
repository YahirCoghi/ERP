namespace MiniERP.Domain.Entities.Common;

public class EntitySequence : BaseEntity
{
    public string EntityName { get; set; } = string.Empty;
    public string Prefix { get; set; } = string.Empty;
    public int LastNumber { get; set; }
    public int Padding { get; set; } = 6;
}
