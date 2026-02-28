using System;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using MiniERP.Application.Contracts;
using MiniERP.Application.Services;
using MiniERP.Infrastructure.Data;
using MiniERP.Infrastructure.Services;
using MiniERP.API.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var dbProvider = builder.Configuration["DatabaseProvider"] ?? Environment.GetEnvironmentVariable("DatabaseProvider") ?? "Sqlite";
var defaultConn = builder.Configuration.GetConnectionString("DefaultConnection");
var masterConn = builder.Configuration.GetConnectionString("MasterConnection") ?? builder.Configuration.GetConnectionString("SqlServer") ?? defaultConn;

builder.Services.AddDbContext<MasterDbContext>(options =>
{
    options.UseSqlServer(masterConn);
});

builder.Services.AddScoped<ITenantAccessor, TenantAccessor>();
builder.Services.AddDbContext<ApplicationDbContext>((sp, options) =>
{
    var accessor = sp.GetRequiredService<ITenantAccessor>();
    if (string.IsNullOrWhiteSpace(accessor.ConnectionString))
    {
        options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer") ?? defaultConn);
        return;
    }
    options.UseSqlServer(accessor.ConnectionString);
});

// JWT Configuration
var jwtKey = builder.Configuration["Jwt:Key"] ?? "MiniERP-SecretKey-2024-ChangeInProduction!";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "MiniERP";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "MiniERP-Client";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization(options =>
{
    // Protect all endpoints by default. Controllers can opt-out with [AllowAnonymous].
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Register application services and repositories
builder.Services.AddScoped(typeof(MiniERP.Application.Contracts.IProductRepository), typeof(MiniERP.Infrastructure.Repositories.ProductRepository));
builder.Services.AddScoped<MiniERP.Application.Services.IProductService, MiniERP.Application.Services.ProductService>();
builder.Services.AddScoped<MiniERP.Application.Contracts.IProductTransactionRepository, MiniERP.Infrastructure.Repositories.ProductTransactionRepository>();
builder.Services.AddScoped<MiniERP.Application.Services.IProductTransactionService, MiniERP.Application.Services.ProductTransactionService>();
builder.Services.AddScoped<IAccountingService, AccountingService>();
builder.Services.AddScoped<IElectronicInvoiceService, ElectronicInvoiceService>();
builder.Services.AddScoped<IElectronicInvoiceSigner, ElectronicInvoiceSignerStub>();
builder.Services.AddHttpClient<IHaciendaClient, HaciendaClient>();
builder.Services.AddSingleton<ElectronicInvoiceXmlBuilder>();
builder.Services.AddScoped<IElectronicDocumentValidator, ElectronicDocumentValidator>();
builder.Services.Configure<HaciendaOptions>(builder.Configuration.GetSection("Hacienda"));
builder.Services.Configure<TenantProvisioningOptions>(builder.Configuration.GetSection("TenantProvisioning"));
builder.Services.AddScoped<TenantProvisioningService>();
builder.Services.AddScoped<LicenseService>();
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHsts();
}

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
        var problem = new Microsoft.AspNetCore.Mvc.ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "Unhandled server error.",
            Detail = app.Environment.IsDevelopment() ? exception?.Message : "An unexpected error occurred."
        };

        context.Response.StatusCode = problem.Status.Value;
        await context.Response.WriteAsJsonAsync(problem);
    });
});

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseMiddleware<TenantResolutionMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
