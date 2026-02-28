using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MiniERP.API.Controllers;
using MiniERP.Domain.Entities.Auth;
using MiniERP.Infrastructure.Data;

namespace MiniERP.API.Tests;

public class AuthControllerTests
{
    private static IConfiguration BuildConfig()
    {
        var values = new Dictionary<string, string?>
        {
            ["Jwt:Key"] = "MiniERP-SecretKey-2024-ChangeInProduction!",
            ["Jwt:Issuer"] = "MiniERP",
            ["Jwt:Audience"] = "MiniERP-Client"
        };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();
    }

    private static ApplicationDbContext BuildDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task Register_CreatesUser_AndReturnsToken()
    {
        await using var db = BuildDbContext();
        var controller = new AuthController(db, BuildConfig());

        var result = await controller.Register(new RegisterRequest("owner1", "owner1@test.com", "P@ssw0rd123"));

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var payload = Assert.IsType<AuthResponse>(ok.Value);
        Assert.False(string.IsNullOrWhiteSpace(payload.Token));
        Assert.Equal("owner1", payload.Username);
        Assert.Equal("User", payload.Role);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsToken()
    {
        await using var db = BuildDbContext();
        db.Users.Add(new User
        {
            Username = "owner2",
            Email = "owner2@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Secret123"),
            Role = "Owner",
            IsActive = true
        });
        await db.SaveChangesAsync();

        var controller = new AuthController(db, BuildConfig());
        var result = await controller.Login(new LoginRequest("owner2", "Secret123"));

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var payload = Assert.IsType<AuthResponse>(ok.Value);
        Assert.Equal("owner2", payload.Username);
        Assert.Equal("Owner", payload.Role);
        Assert.False(string.IsNullOrWhiteSpace(payload.Token));
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ReturnsUnauthorized()
    {
        await using var db = BuildDbContext();
        db.Users.Add(new User
        {
            Username = "owner3",
            Email = "owner3@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Secret123"),
            Role = "Owner",
            IsActive = true
        });
        await db.SaveChangesAsync();

        var controller = new AuthController(db, BuildConfig());
        var result = await controller.Login(new LoginRequest("owner3", "WrongPassword"));

        Assert.IsType<UnauthorizedObjectResult>(result.Result);
    }
}
