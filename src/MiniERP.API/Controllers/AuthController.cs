using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MiniERP.Application.Contracts;
using MiniERP.Domain.Entities.Auth;
using MiniERP.Infrastructure.Data;

namespace MiniERP.API.Controllers;

public record LoginRequest(string Username, string Password);
public record RegisterRequest(string Username, string Email, string Password);
public record AuthResponse(string Token, string Username, string Role);

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly MasterDbContext _masterDb;
    private readonly ITenantAccessor _tenantAccessor;
    private readonly IConfiguration _configuration;

    public AuthController(
        ApplicationDbContext context,
        MasterDbContext masterDb,
        ITenantAccessor tenantAccessor,
        IConfiguration configuration)
    {
        _context = context;
        _masterDb = masterDb;
        _tenantAccessor = tenantAccessor;
        _configuration = configuration;
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == request.Username && u.IsActive);

        if (user == null || !VerifyPassword(request.Password, user.PasswordHash))
        {
            return Unauthorized(new { message = "Usuario o contraseña incorrectos" });
        }

        user.LastLoginAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        var token = GenerateJwtToken(user);
        return Ok(new AuthResponse(token, user.Username, user.Role));
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
    {
        if (_tenantAccessor.TenantId <= 0)
        {
            return BadRequest(new { message = "Tenant context is required." });
        }

        var tenant = await _masterDb.Tenants
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == _tenantAccessor.TenantId && t.IsActive);
        if (tenant == null)
        {
            return NotFound(new { message = "Tenant not found." });
        }

        var activeUsers = await _context.Users.CountAsync(u => u.IsActive);
        if (activeUsers >= tenant.MaxUsers)
        {
            return StatusCode(StatusCodes.Status402PaymentRequired, new
            {
                message = $"User limit reached for current plan ({tenant.MaxUsers})."
            });
        }

        if (await _context.Users.AnyAsync(u => u.Username == request.Username))
        {
            return BadRequest(new { message = "El usuario ya existe" });
        }

        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
        {
            return BadRequest(new { message = "El email ya está registrado" });
        }

        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = HashPassword(request.Password),
            Role = "User"
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var token = GenerateJwtToken(user);
        return Ok(new AuthResponse(token, user.Username, user.Role));
    }

    private string GenerateJwtToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            _configuration["Jwt:Key"] ?? "MiniERP-SecretKey-2024-ChangeInProduction!"));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"] ?? "MiniERP",
            audience: _configuration["Jwt:Audience"] ?? "MiniERP-Client",
            claims: claims,
            expires: DateTime.Now.AddHours(8),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    private static bool VerifyPassword(string password, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }
}
