using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using MiniERP.Application.Contracts;
using MiniERP.Domain.Entities.MultiTenancy;
using MiniERP.Infrastructure.Data;

namespace MiniERP.API.Middleware;

public class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;

    public TenantResolutionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, MasterDbContext masterDb, ITenantAccessor accessor, Microsoft.Extensions.Configuration.IConfiguration configuration, IHostEnvironment env)
    {
        var path = context.Request.Path.Value?.ToLowerInvariant() ?? string.Empty;
        if (path.StartsWith("/api/tenants") || path.StartsWith("/api/signup") || path.StartsWith("/health"))
        {
            await _next(context);
            return;
        }

        var allowMissing = configuration["TenantDefaults:AllowMissingHeader"] == "true" && env.IsDevelopment();
        var defaultTenantIdText = configuration["TenantDefaults:DefaultTenantId"];
        int? defaultTenantId = int.TryParse(defaultTenantIdText, out var parsed) ? parsed : null;

        if (!context.Request.Headers.TryGetValue("X-Tenant-ID", out var tenantHeader))
        {
            if (allowMissing && defaultTenantId.HasValue)
            {
                tenantHeader = defaultTenantId.Value.ToString();
            }
            else
            {
                await WriteProblemAsync(context, StatusCodes.Status400BadRequest, "Missing X-Tenant-ID header.");
                return;
            }
        }

        if (!int.TryParse(tenantHeader.FirstOrDefault(), out var tenantId))
        {
            await WriteProblemAsync(context, StatusCodes.Status400BadRequest, "Invalid X-Tenant-ID header.");
            return;
        }

        var tenant = await masterDb.Tenants.FirstOrDefaultAsync(t => t.Id == tenantId && t.IsActive);
        if (tenant == null)
        {
            await WriteProblemAsync(context, StatusCodes.Status404NotFound, "Tenant not found.");
            return;
        }

        var now = DateTime.UtcNow;
        var effectiveStatus = tenant.Status;

        if (tenant.Status == SubscriptionStatus.Trial &&
            tenant.TrialEndsAt.HasValue &&
            tenant.TrialEndsAt.Value < now)
        {
            effectiveStatus = SubscriptionStatus.PastDue;
        }

        if (tenant.Status == SubscriptionStatus.Active &&
            tenant.PaidUntil.HasValue &&
            tenant.PaidUntil.Value < now)
        {
            effectiveStatus = SubscriptionStatus.PastDue;
        }

        if (effectiveStatus != tenant.Status)
        {
            tenant.Status = effectiveStatus;
            tenant.UpdatedAt = now;

            var license = await masterDb.Licenses.FirstOrDefaultAsync(l => l.TenantId == tenant.Id);
            if (license != null)
            {
                license.Status = effectiveStatus;
                license.UpdatedAt = now;
            }

            await masterDb.SaveChangesAsync();
        }

        if (effectiveStatus is SubscriptionStatus.Suspended
            or SubscriptionStatus.Canceled
            or SubscriptionStatus.PastDue)
        {
            await WriteProblemAsync(
                context,
                StatusCodes.Status402PaymentRequired,
                $"Tenant subscription inactive ({effectiveStatus}).");
            return;
        }

        accessor.TenantId = tenant.Id;
        accessor.TenantName = tenant.Name;
        accessor.ConnectionString = tenant.ConnectionString;

        await _next(context);
    }

    private static Task WriteProblemAsync(HttpContext context, int statusCode, string detail)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";
        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = "Tenant resolution failed.",
            Detail = detail
        };
        return context.Response.WriteAsJsonAsync(problem);
    }
}
