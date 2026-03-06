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
[Authorize(Roles = AppRoles.Sales)]
[Authorize(Policy = "Campaigns:Read")]
public class CampaignsController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public CampaignsController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var rows = await _db.Campaigns
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new CampaignDto(x.Id, x.Name, x.SegmentCriteriaJson, x.ScheduledAt, x.Status))
            .ToListAsync();
        return Ok(rows);
    }

    [HttpPost]
    [Authorize(Policy = "Campaigns:Full")]
    public async Task<IActionResult> Create([FromBody] CampaignDto request)
    {
        var row = new Campaign
        {
            Name = request.Name.Trim(),
            SegmentCriteriaJson = string.IsNullOrWhiteSpace(request.SegmentCriteriaJson) ? "{}" : request.SegmentCriteriaJson,
            ScheduledAt = request.ScheduledAt,
            Status = string.IsNullOrWhiteSpace(request.Status) ? "Draft" : request.Status.Trim()
        };
        _db.Campaigns.Add(row);
        await _db.SaveChangesAsync();
        return Ok(new CampaignDto(row.Id, row.Name, row.SegmentCriteriaJson, row.ScheduledAt, row.Status));
    }

    [HttpPost("{id:int}/wizard/segment")]
    [Authorize(Policy = "Campaigns:Full")]
    public async Task<IActionResult> RunSegmentation(int id, [FromQuery] int? maxCustomers = 200)
    {
        var campaign = await _db.Campaigns.FirstOrDefaultAsync(x => x.Id == id && x.IsActive);
        if (campaign == null)
            return NotFound(new { message = "Campaign not found." });

        var already = await _db.CampaignRecipients.Where(x => x.CampaignId == id).ToListAsync();
        if (already.Count > 0)
            _db.CampaignRecipients.RemoveRange(already);

        var selectedCustomers = await _db.Customers
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .Take(Math.Max(maxCustomers ?? 200, 1))
            .Select(x => x.Id)
            .ToListAsync();

        foreach (var customerId in selectedCustomers)
        {
            _db.CampaignRecipients.Add(new CampaignRecipient
            {
                CampaignId = id,
                CustomerId = customerId,
                Channel = "Email",
                Delivered = false,
                Opened = false,
                Converted = false
            });
        }

        campaign.Status = "Segmented";
        campaign.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Ok(new { campaignId = id, recipients = selectedCustomers.Count, status = campaign.Status });
    }

    [HttpPost("{id:int}/schedule")]
    [Authorize(Policy = "Campaigns:Full")]
    public async Task<IActionResult> Schedule(int id, [FromQuery] DateTime scheduledAt)
    {
        var campaign = await _db.Campaigns.FirstOrDefaultAsync(x => x.Id == id && x.IsActive);
        if (campaign == null)
            return NotFound(new { message = "Campaign not found." });

        campaign.ScheduledAt = scheduledAt;
        campaign.Status = "Scheduled";
        campaign.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(new CampaignDto(campaign.Id, campaign.Name, campaign.SegmentCriteriaJson, campaign.ScheduledAt, campaign.Status));
    }

    [HttpPost("{id:int}/run")]
    [Authorize(Policy = "Campaigns:Full")]
    public async Task<IActionResult> Run(int id)
    {
        var campaign = await _db.Campaigns.FirstOrDefaultAsync(x => x.Id == id && x.IsActive);
        if (campaign == null)
            return NotFound(new { message = "Campaign not found." });

        var recipients = await _db.CampaignRecipients.Where(x => x.CampaignId == id && x.IsActive).ToListAsync();
        foreach (var r in recipients)
        {
            r.Delivered = true;
            r.Opened = true;
            r.Converted = false;
            r.UpdatedAt = DateTime.UtcNow;
        }

        campaign.Status = "Executed";
        campaign.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Ok(new
        {
            campaignId = id,
            delivered = recipients.Count,
            opened = recipients.Count(x => x.Opened),
            converted = recipients.Count(x => x.Converted)
        });
    }

    [HttpGet("{id:int}/stats")]
    public async Task<IActionResult> Stats(int id)
    {
        var recipients = await _db.CampaignRecipients
            .AsNoTracking()
            .Where(x => x.CampaignId == id && x.IsActive)
            .ToListAsync();

        return Ok(new
        {
            Total = recipients.Count,
            Delivered = recipients.Count(x => x.Delivered),
            Opened = recipients.Count(x => x.Opened),
            Converted = recipients.Count(x => x.Converted)
        });
    }
}
