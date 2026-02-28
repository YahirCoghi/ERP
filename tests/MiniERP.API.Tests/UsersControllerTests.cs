using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniERP.API.Controllers;
using MiniERP.Domain.Entities.Auth;
using MiniERP.Infrastructure.Data;

namespace MiniERP.API.Tests;

public class UsersControllerTests
{
    private static ApplicationDbContext BuildDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task Get_ReturnsUsers()
    {
        await using var db = BuildDbContext();
        db.Users.AddRange(
            new User { Username = "a", Email = "a@test.com", PasswordHash = "x", Role = "Sales", IsActive = true },
            new User { Username = "b", Email = "b@test.com", PasswordHash = "x", Role = "Owner", IsActive = true });
        await db.SaveChangesAsync();

        var controller = new UsersController(db);
        var result = await controller.Get();
        var ok = Assert.IsType<OkObjectResult>(result);
        var users = Assert.IsAssignableFrom<IEnumerable<UserSummaryResponse>>(ok.Value);
        Assert.Equal(2, users.Count());
    }

    [Fact]
    public async Task UpdateRole_ChangesRole()
    {
        await using var db = BuildDbContext();
        var user = new User { Username = "c", Email = "c@test.com", PasswordHash = "x", Role = "Sales", IsActive = true };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var controller = new UsersController(db);
        var result = await controller.UpdateRole(user.Id, new UpdateUserRoleRequest("Accounting"));

        Assert.IsType<NoContentResult>(result);
        var updated = await db.Users.FirstAsync(u => u.Id == user.Id);
        Assert.Equal("Accounting", updated.Role);
    }

    [Fact]
    public async Task ResetPassword_UpdatesHash()
    {
        await using var db = BuildDbContext();
        var user = new User
        {
            Username = "d",
            Email = "d@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("oldpass123"),
            Role = "Owner",
            IsActive = true
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var oldHash = user.PasswordHash;
        var controller = new UsersController(db);
        var result = await controller.ResetPassword(user.Id, new ResetPasswordRequest("NewPass123"));

        Assert.IsType<NoContentResult>(result);
        var updated = await db.Users.FirstAsync(u => u.Id == user.Id);
        Assert.NotEqual(oldHash, updated.PasswordHash);
        Assert.True(BCrypt.Net.BCrypt.Verify("NewPass123", updated.PasswordHash));
    }
}
