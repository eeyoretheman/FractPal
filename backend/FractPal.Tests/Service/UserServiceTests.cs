using FractPal.Data;
using FractPal.Model.DTO.User;
using FractPal.Model.Entities;
using FractPal.Service.Implementation;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FractPal.Tests.Services;

public class UserServiceTests
{
    private DbContextOptions<ApplicationDbContext> GetDbContextOptions()
    {
        return new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    [Fact]
    public async Task GetProfileAsync_InvalidUserIdFormat_ThrowsArgumentException()
    {
        using var context = new ApplicationDbContext(GetDbContextOptions());
        var service = new UserService(context);

        await Assert.ThrowsAsync<ArgumentException>(() => service.GetProfileAsync("invalid", Guid.NewGuid().ToString()));
    }

    [Fact]
    public async Task GetProfileAsync_UserNotFound_ThrowsKeyNotFoundException()
    {
        using var context = new ApplicationDbContext(GetDbContextOptions());
        var service = new UserService(context);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.GetProfileAsync(Guid.NewGuid().ToString(), Guid.NewGuid().ToString()));
    }

    [Fact]
    public async Task GetProfileAsync_Success_ReturnsProfile()
    {
        using var context = new ApplicationDbContext(GetDbContextOptions());
        var service = new UserService(context);
        var targetUserId = Guid.NewGuid();
        var currentUserId = Guid.NewGuid();

        context.Users.Add(new FractPalUser { Id = targetUserId, UserName = "target" });
        context.Follows.Add(new Model.Entities.Follow { FollowerId = currentUserId, FollowingId = targetUserId });
        await context.SaveChangesAsync();

        var result = await service.GetProfileAsync(targetUserId.ToString(), currentUserId.ToString());

        Assert.NotNull(result);
        Assert.Equal("target", result.Username);
        Assert.True(result.IsFollowedByCurrentUser);
    }

    [Fact]
    public async Task UpdateProfileAsync_InvalidUserIdFormat_ThrowsArgumentException()
    {
        using var context = new ApplicationDbContext(GetDbContextOptions());
        var service = new UserService(context);

        await Assert.ThrowsAsync<ArgumentException>(() => service.UpdateProfileAsync("invalid", new UpdateProfileRequest()));
    }

    [Fact]
    public async Task UpdateProfileAsync_UserNotFound_ThrowsKeyNotFoundException()
    {
        using var context = new ApplicationDbContext(GetDbContextOptions());
        var service = new UserService(context);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.UpdateProfileAsync(Guid.NewGuid().ToString(), new UpdateProfileRequest()));
    }

    [Fact]
    public async Task UpdateProfileAsync_Success_UpdatesBio()
    {
        using var context = new ApplicationDbContext(GetDbContextOptions());
        var service = new UserService(context);
        var userId = Guid.NewGuid();

        context.Users.Add(new FractPalUser { Id = userId, UserName = "user", Bio = "Old Bio" });
        await context.SaveChangesAsync();

        var req = new UpdateProfileRequest { Bio = "New Bio" };
        var result = await service.UpdateProfileAsync(userId.ToString(), req);

        Assert.NotNull(result);
        Assert.Equal("New Bio", result.Bio);

        var dbUser = await context.Users.FindAsync(userId);
        Assert.Equal("New Bio", dbUser!.Bio);
    }

    [Fact]
    public async Task ToggleFollowAsync_InvalidUserIdFormat_ThrowsArgumentException()
    {
        using var context = new ApplicationDbContext(GetDbContextOptions());
        var service = new UserService(context);

        await Assert.ThrowsAsync<ArgumentException>(() => service.ToggleFollowAsync("invalid", Guid.NewGuid().ToString()));
    }

    [Fact]
    public async Task ToggleFollowAsync_FollowYourself_ThrowsInvalidOperationException()
    {
        using var context = new ApplicationDbContext(GetDbContextOptions());
        var service = new UserService(context);
        var userId = Guid.NewGuid().ToString();

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.ToggleFollowAsync(userId, userId));
    }

    [Fact]
    public async Task ToggleFollowAsync_NewFollow_ReturnsTrue()
    {
        using var context = new ApplicationDbContext(GetDbContextOptions());
        var service = new UserService(context);
        var followerId = Guid.NewGuid();
        var followingId = Guid.NewGuid();

        var result = await service.ToggleFollowAsync(followerId.ToString(), followingId.ToString());

        Assert.True(result);
        var follow = await context.Follows.FirstOrDefaultAsync(f => f.FollowerId == followerId && f.FollowingId == followingId);
        Assert.NotNull(follow);
    }

    [Fact]
    public async Task ToggleFollowAsync_ExistingFollow_UnfollowsReturnsFalse()
    {
        using var context = new ApplicationDbContext(GetDbContextOptions());
        var service = new UserService(context);
        var followerId = Guid.NewGuid();
        var followingId = Guid.NewGuid();

        context.Follows.Add(new Model.Entities.Follow { FollowerId = followerId, FollowingId = followingId });
        await context.SaveChangesAsync();

        var result = await service.ToggleFollowAsync(followerId.ToString(), followingId.ToString());

        Assert.False(result);
        var follow = await context.Follows.FirstOrDefaultAsync(f => f.FollowerId == followerId && f.FollowingId == followingId);
        Assert.Null(follow);
    }

    [Fact]
    public async Task SearchUsersAsync_InvalidUserIdFormat_ThrowsArgumentException()
    {
        using var context = new ApplicationDbContext(GetDbContextOptions());
        var service = new UserService(context);

        await Assert.ThrowsAsync<ArgumentException>(() => service.SearchUsersAsync("query", "invalid"));
    }

    [Fact]
    public async Task SearchUsersAsync_ReturnsMatchingUsers()
    {
        using var context = new ApplicationDbContext(GetDbContextOptions());
        var service = new UserService(context);
        var currentUserId = Guid.NewGuid();

        context.Users.Add(new FractPalUser { Id = Guid.NewGuid(), UserName = "test_user1" });
        context.Users.Add(new FractPalUser { Id = Guid.NewGuid(), UserName = "test_user2" });
        context.Users.Add(new FractPalUser { Id = Guid.NewGuid(), UserName = "other_user" });
        await context.SaveChangesAsync();

        var result = await service.SearchUsersAsync("test", currentUserId.ToString());

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Contains(result, u => u.Username == "test_user1");
        Assert.Contains(result, u => u.Username == "test_user2");
    }
}
