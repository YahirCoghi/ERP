using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniERP.API.Security;
using MiniERP.Application.DTOs;
using MiniERP.Domain.Entities.Advanced;
using MiniERP.Infrastructure.Data;

namespace MiniERP.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = AppRoles.OwnerAdmin)]
public class ServiceAndToolsController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public ServiceAndToolsController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet("knowledge-base")]
    [Authorize(Policy = "KnowledgeBase:Read")]
    public async Task<IActionResult> GetKnowledgeBase([FromQuery] string? q = null)
    {
        var query = _db.KnowledgeBaseArticles.AsNoTracking().Where(x => x.IsActive);
        if (!string.IsNullOrWhiteSpace(q))
        {
            var text = q.Trim();
            query = query.Where(x => x.Title.Contains(text) || x.Content.Contains(text) || (x.Tags != null && x.Tags.Contains(text)));
        }

        var rows = await query
            .OrderByDescending(x => x.UpdatedAt ?? x.CreatedAt)
            .Take(300)
            .Select(x => new KnowledgeBaseArticleDto(x.Id, x.Title, x.Category, x.Content, x.Tags))
            .ToListAsync();
        return Ok(rows);
    }

    [HttpPost("knowledge-base")]
    [Authorize(Policy = "KnowledgeBase:Full")]
    public async Task<IActionResult> CreateKnowledgeBase([FromBody] KnowledgeBaseArticleDto request)
    {
        var row = new KnowledgeBaseArticle
        {
            Title = request.Title.Trim(),
            Category = string.IsNullOrWhiteSpace(request.Category) ? "General" : request.Category.Trim(),
            Content = request.Content,
            Tags = request.Tags
        };
        _db.KnowledgeBaseArticles.Add(row);
        await _db.SaveChangesAsync();
        return Ok(new KnowledgeBaseArticleDto(row.Id, row.Title, row.Category, row.Content, row.Tags));
    }

    [HttpGet("service-reports")]
    [Authorize(Policy = "ServiceReports:Read")]
    public async Task<IActionResult> GetServiceReports()
    {
        var rows = await _db.ServiceSlaMetrics
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderByDescending(x => x.MetricDate)
            .Take(90)
            .Select(x => new ServiceSlaMetricDto(x.Id, x.MetricDate, x.TicketsOpened, x.TicketsResolved, x.AvgResolutionHours, x.SlaCompliancePct))
            .ToListAsync();

        var dashboard = new
        {
            Last30Days = rows.Take(30),
            AvgSla = rows.Count == 0 ? 0 : rows.Average(x => x.SlaCompliancePct),
            AvgResolutionHours = rows.Count == 0 ? 0 : rows.Average(x => x.AvgResolutionHours)
        };
        return Ok(dashboard);
    }

    [HttpPost("service-reports")]
    [Authorize(Policy = "ServiceReports:Full")]
    public async Task<IActionResult> CreateServiceMetric([FromBody] ServiceSlaMetricDto request)
    {
        var row = new ServiceSlaMetric
        {
            MetricDate = request.MetricDate.Date,
            TicketsOpened = request.TicketsOpened,
            TicketsResolved = request.TicketsResolved,
            AvgResolutionHours = request.AvgResolutionHours,
            SlaCompliancePct = request.SlaCompliancePct
        };
        _db.ServiceSlaMetrics.Add(row);
        await _db.SaveChangesAsync();
        return Ok(new ServiceSlaMetricDto(row.Id, row.MetricDate, row.TicketsOpened, row.TicketsResolved, row.AvgResolutionHours, row.SlaCompliancePct));
    }

    [HttpGet("query-manager")]
    [Authorize(Policy = "QueryManager:Read")]
    public async Task<IActionResult> GetQueries()
    {
        var rows = await _db.QueryDefinitions
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .Select(x => new QueryDefinitionDto(x.Id, x.Name, x.SqlText, x.Module))
            .ToListAsync();
        return Ok(rows);
    }

    [HttpPost("query-manager")]
    [Authorize(Policy = "QueryManager:Full")]
    public async Task<IActionResult> SaveQuery([FromBody] QueryDefinitionDto request)
    {
        var row = await _db.QueryDefinitions.FirstOrDefaultAsync(x => x.Id == request.Id && x.IsActive);
        if (row == null)
        {
            row = new QueryDefinition
            {
                Name = request.Name.Trim(),
                SqlText = request.SqlText,
                Module = string.IsNullOrWhiteSpace(request.Module) ? "General" : request.Module.Trim()
            };
            _db.QueryDefinitions.Add(row);
        }
        else
        {
            row.Name = request.Name.Trim();
            row.SqlText = request.SqlText;
            row.Module = string.IsNullOrWhiteSpace(request.Module) ? "General" : request.Module.Trim();
            row.UpdatedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
        return Ok(new QueryDefinitionDto(row.Id, row.Name, row.SqlText, row.Module));
    }

    [HttpGet("print-layouts")]
    [Authorize(Policy = "PrintLayouts:Read")]
    public async Task<IActionResult> GetPrintLayouts()
    {
        var rows = await _db.PrintLayoutTemplates
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .Select(x => new PrintLayoutTemplateDto(x.Id, x.Name, x.DocumentType, x.LayoutJson))
            .ToListAsync();
        return Ok(rows);
    }

    [HttpPost("print-layouts")]
    [Authorize(Policy = "PrintLayouts:Full")]
    public async Task<IActionResult> SavePrintLayout([FromBody] PrintLayoutTemplateDto request)
    {
        var row = await _db.PrintLayoutTemplates.FirstOrDefaultAsync(x => x.Id == request.Id && x.IsActive);
        if (row == null)
        {
            row = new PrintLayoutTemplate
            {
                Name = request.Name.Trim(),
                DocumentType = request.DocumentType.Trim(),
                LayoutJson = request.LayoutJson
            };
            _db.PrintLayoutTemplates.Add(row);
        }
        else
        {
            row.Name = request.Name.Trim();
            row.DocumentType = request.DocumentType.Trim();
            row.LayoutJson = request.LayoutJson;
            row.UpdatedAt = DateTime.UtcNow;
        }
        await _db.SaveChangesAsync();
        return Ok(new PrintLayoutTemplateDto(row.Id, row.Name, row.DocumentType, row.LayoutJson));
    }

    [HttpGet("udf")]
    [Authorize(Policy = "UserDefinedObjects:Read")]
    public async Task<IActionResult> GetUserDefinedFields()
    {
        var rows = await _db.UserDefinedFields
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.TargetEntity)
            .ThenBy(x => x.FieldName)
            .Select(x => new UserDefinedFieldDto(x.Id, x.TargetEntity, x.FieldName, x.DataType, x.Required))
            .ToListAsync();
        return Ok(rows);
    }

    [HttpPost("udf")]
    [Authorize(Policy = "UserDefinedObjects:Full")]
    public async Task<IActionResult> SaveUserDefinedField([FromBody] UserDefinedFieldDto request)
    {
        var row = await _db.UserDefinedFields.FirstOrDefaultAsync(x => x.Id == request.Id && x.IsActive);
        if (row == null)
        {
            row = new UserDefinedField
            {
                TargetEntity = request.TargetEntity.Trim(),
                FieldName = request.FieldName.Trim(),
                DataType = request.DataType.Trim(),
                Required = request.Required
            };
            _db.UserDefinedFields.Add(row);
        }
        else
        {
            row.TargetEntity = request.TargetEntity.Trim();
            row.FieldName = request.FieldName.Trim();
            row.DataType = request.DataType.Trim();
            row.Required = request.Required;
            row.UpdatedAt = DateTime.UtcNow;
        }
        await _db.SaveChangesAsync();
        return Ok(new UserDefinedFieldDto(row.Id, row.TargetEntity, row.FieldName, row.DataType, row.Required));
    }

    [HttpGet("udo")]
    [Authorize(Policy = "UserDefinedObjects:Read")]
    public async Task<IActionResult> GetUserDefinedObjects()
    {
        var rows = await _db.UserDefinedObjects
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.ObjectName)
            .Select(x => new UserDefinedObjectDto(x.Id, x.ObjectName, x.SchemaJson))
            .ToListAsync();
        return Ok(rows);
    }

    [HttpPost("udo")]
    [Authorize(Policy = "UserDefinedObjects:Full")]
    public async Task<IActionResult> SaveUserDefinedObject([FromBody] UserDefinedObjectDto request)
    {
        var row = await _db.UserDefinedObjects.FirstOrDefaultAsync(x => x.Id == request.Id && x.IsActive);
        if (row == null)
        {
            row = new UserDefinedObject
            {
                ObjectName = request.ObjectName.Trim(),
                SchemaJson = request.SchemaJson
            };
            _db.UserDefinedObjects.Add(row);
        }
        else
        {
            row.ObjectName = request.ObjectName.Trim();
            row.SchemaJson = request.SchemaJson;
            row.UpdatedAt = DateTime.UtcNow;
        }
        await _db.SaveChangesAsync();
        return Ok(new UserDefinedObjectDto(row.Id, row.ObjectName, row.SchemaJson));
    }
}
