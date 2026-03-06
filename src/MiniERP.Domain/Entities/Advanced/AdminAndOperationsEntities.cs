using System;
using System.Collections.Generic;
using MiniERP.Domain.Entities.Common;

namespace MiniERP.Domain.Entities.Advanced;

public class ApprovalTemplate : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ICollection<ApprovalTemplateStage> Stages { get; set; } = new List<ApprovalTemplateStage>();
}

public class ApprovalTemplateStage : BaseEntity
{
    public int TemplateId { get; set; }
    public ApprovalTemplate? Template { get; set; }
    public int StageOrder { get; set; }
    public string Name { get; set; } = string.Empty;
    public string RoleRequired { get; set; } = "Admin";
    public decimal? MinAmount { get; set; }
}

public class AddonDefinition : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = "1.0.0";
    public string? ConfigurationSchemaJson { get; set; }
}

public class AddonActivation : BaseEntity
{
    public int AddonDefinitionId { get; set; }
    public AddonDefinition? AddonDefinition { get; set; }
    public bool Enabled { get; set; } = true;
    public string? ConfigurationJson { get; set; }
    public DateTime? EnabledAt { get; set; }
}

public class MobileServiceConfig : BaseEntity
{
    public string Provider { get; set; } = "NEX Mobile";
    public int MaxDevices { get; set; } = 5;
    public bool Enabled { get; set; } = true;
    public DateTime? LicenseExpiresAt { get; set; }
}

public class WorkflowDefinition : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
    public bool IsPublished { get; set; }
    public ICollection<WorkflowStep> Steps { get; set; } = new List<WorkflowStep>();
}

public class WorkflowStep : BaseEntity
{
    public int WorkflowDefinitionId { get; set; }
    public WorkflowDefinition? WorkflowDefinition { get; set; }
    public int StepOrder { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ActionType { get; set; } = "Approval";
    public string? ConditionsJson { get; set; }
}

public class WorkflowInstance : BaseEntity
{
    public int WorkflowDefinitionId { get; set; }
    public WorkflowDefinition? WorkflowDefinition { get; set; }
    public string ReferenceType { get; set; } = string.Empty;
    public int ReferenceId { get; set; }
    public string Status { get; set; } = "Running";
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
}

public class TenantLicenseAssignment : BaseEntity
{
    public string LicenseCode { get; set; } = string.Empty;
    public string Scope { get; set; } = "Tenant";
    public int? UserId { get; set; }
    public bool Enabled { get; set; } = true;
    public DateTime? ExpiresAt { get; set; }
}

public class ProcurementConfirmation : BaseEntity
{
    public string ReferenceType { get; set; } = "ProductionOrder";
    public int ReferenceId { get; set; }
    public string Status { get; set; } = "Draft";
    public bool IsPartial { get; set; }
    public string? Notes { get; set; }
    public ICollection<ProcurementConfirmationLine> Lines { get; set; } = new List<ProcurementConfirmationLine>();
}

public class ProcurementConfirmationLine : BaseEntity
{
    public int ProcurementConfirmationId { get; set; }
    public ProcurementConfirmation? ProcurementConfirmation { get; set; }
    public int ProductId { get; set; }
    public decimal RequestedQty { get; set; }
    public decimal ConfirmedQty { get; set; }
}

public class PickPackTask : BaseEntity
{
    public string TaskNumber { get; set; } = string.Empty;
    public string TaskType { get; set; } = "Pick";
    public string Status { get; set; } = "Open";
    public string? Warehouse { get; set; }
    public ICollection<PickPackTaskLine> Lines { get; set; } = new List<PickPackTaskLine>();
}

public class PickPackTaskLine : BaseEntity
{
    public int PickPackTaskId { get; set; }
    public PickPackTask? PickPackTask { get; set; }
    public int ProductId { get; set; }
    public decimal Quantity { get; set; }
    public decimal ConfirmedQuantity { get; set; }
}
