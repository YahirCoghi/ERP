using System;
using System.Collections.Generic;
using MiniERP.Domain.Entities.Common;

namespace MiniERP.Domain.Entities.Advanced;

public class Vendor1099Amount : BaseEntity
{
    public int SupplierId { get; set; }
    public int Year { get; set; }
    public decimal NonEmployeeCompensation { get; set; }
    public decimal FederalTaxWithheld { get; set; }
    public string? Notes { get; set; }
}

public class FixedAsset : BaseEntity
{
    public string AssetCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateTime AcquisitionDate { get; set; }
    public decimal AcquisitionCost { get; set; }
    public decimal ResidualValue { get; set; }
    public int UsefulLifeMonths { get; set; } = 60;
    public string Status { get; set; } = "Active";
}

public class FixedAssetDepreciation : BaseEntity
{
    public int FixedAssetId { get; set; }
    public FixedAsset? FixedAsset { get; set; }
    public DateTime PeriodDate { get; set; }
    public decimal Amount { get; set; }
}

public class IntrastatDeclaration : BaseEntity
{
    public string DeclarationNumber { get; set; } = string.Empty;
    public int Year { get; set; }
    public int Month { get; set; }
    public string Status { get; set; } = "Draft";
    public ICollection<IntrastatDeclarationLine> Lines { get; set; } = new List<IntrastatDeclarationLine>();
}

public class IntrastatDeclarationLine : BaseEntity
{
    public int IntrastatDeclarationId { get; set; }
    public IntrastatDeclaration? IntrastatDeclaration { get; set; }
    public string CommodityCode { get; set; } = string.Empty;
    public string CountryCode { get; set; } = string.Empty;
    public decimal NetMassKg { get; set; }
    public decimal ValueAmount { get; set; }
}

public class Campaign : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string SegmentCriteriaJson { get; set; } = "{}";
    public DateTime? ScheduledAt { get; set; }
    public string Status { get; set; } = "Draft";
    public ICollection<CampaignRecipient> Recipients { get; set; } = new List<CampaignRecipient>();
}

public class CampaignRecipient : BaseEntity
{
    public int CampaignId { get; set; }
    public Campaign? Campaign { get; set; }
    public int CustomerId { get; set; }
    public string Channel { get; set; } = "Email";
    public bool Delivered { get; set; }
    public bool Opened { get; set; }
    public bool Converted { get; set; }
}

public class KnowledgeBaseArticle : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = "General";
    public string Content { get; set; } = string.Empty;
    public string? Tags { get; set; }
}

public class ServiceSlaMetric : BaseEntity
{
    public DateTime MetricDate { get; set; } = DateTime.UtcNow.Date;
    public int TicketsOpened { get; set; }
    public int TicketsResolved { get; set; }
    public decimal AvgResolutionHours { get; set; }
    public decimal SlaCompliancePct { get; set; }
}

public class QueryDefinition : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string SqlText { get; set; } = string.Empty;
    public string Module { get; set; } = "General";
}

public class PrintLayoutTemplate : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty;
    public string LayoutJson { get; set; } = "{}";
}

public class UserDefinedField : BaseEntity
{
    public string TargetEntity { get; set; } = string.Empty;
    public string FieldName { get; set; } = string.Empty;
    public string DataType { get; set; } = "string";
    public bool Required { get; set; }
}

public class UserDefinedObject : BaseEntity
{
    public string ObjectName { get; set; } = string.Empty;
    public string SchemaJson { get; set; } = "{}";
}
