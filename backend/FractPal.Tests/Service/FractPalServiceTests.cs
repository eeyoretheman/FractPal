using FractPal.Data;
using FractPal.Model.DTO.Fractal;
using FractPal.Model.Entities;
using FractPal.Service.Implementation;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FractPal.Tests.Services;

public class FractalServiceTests
{
    private DbContextOptions<ApplicationDbContext> GetDbContextOptions()
    {
        return new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    [Fact]
    public async Task GetFeedAsync_ReturnsPaginatedFeed()
    {
        using var context = new ApplicationDbContext(GetDbContextOptions());
        var service = new FractalService(context);
        var userId = Guid.NewGuid();

        context.Users.Add(new FractPalUser { Id = userId, UserName = "user" });
        var fractalId1 = Guid.NewGuid();
        context.Fractals.Add(new Model.Entities.Fractal { Id = fractalId1, Name = "F1", Axiom = "F", Rules = "F", Instructions = "F", Thumbnail = "T", CreatedAt = DateTime.UtcNow, AuthorId = userId });
        context.Posts.Add(new Model.Entities.Post { Id = Guid.NewGuid(), Name = "P1", FractalId = fractalId1, AuthorId = userId });
        var fractalId2 = Guid.NewGuid();
        context.Fractals.Add(new Model.Entities.Fractal { Id = fractalId2, Name = "F2", Axiom = "F", Rules = "F", Instructions = "F", Thumbnail = "T", CreatedAt = DateTime.UtcNow.AddMinutes(-5), AuthorId = userId });
        context.Posts.Add(new Model.Entities.Post { Id = Guid.NewGuid(), Name = "P2", FractalId = fractalId2, AuthorId = userId });
        await context.SaveChangesAsync();

        var feed = await service.GetFeedAsync(userId, 1, 1);

        Assert.NotNull(feed);
        Assert.Equal(2, feed.TotalCount);
        Assert.Single(feed.Fractals);
        Assert.Equal("F1", feed.Fractals[0].Name);
    }

    [Fact]
    public async Task GetUserFractalsAsync_ReturnsUserFractals()
    {
        using var context = new ApplicationDbContext(GetDbContextOptions());
        var service = new FractalService(context);
        var userId = Guid.NewGuid();

        context.Users.Add(new FractPalUser { Id = userId, UserName = "user" });
        context.Fractals.Add(new Model.Entities.Fractal { Id = Guid.NewGuid(), Name = "F1", Axiom = "F", Rules = "F", Instructions = "F", Thumbnail = "T", AuthorId = userId });
        context.Fractals.Add(new Model.Entities.Fractal { Id = Guid.NewGuid(), Name = "F2", Axiom = "F", Rules = "F", Instructions = "F", Thumbnail = "T", AuthorId = Guid.NewGuid() });
        await context.SaveChangesAsync();

        var fractals = await service.GetUserFractalsAsync(userId, userId);

        Assert.Single(fractals);
        Assert.Equal("F1", fractals[0].Name);
    }

    [Fact]
    public async Task GetPublishedFractalsByUserAsync_ReturnsUserFractals()
    {
        using var context = new ApplicationDbContext(GetDbContextOptions());
        var service = new FractalService(context);
        var userId = Guid.NewGuid();

        context.Users.Add(new FractPalUser { Id = userId, UserName = "user" });
        var fractalId = Guid.NewGuid();
        context.Fractals.Add(new Model.Entities.Fractal { Id = fractalId, Name = "F1", Axiom = "F", Rules = "F", Instructions = "F", Thumbnail = "T", AuthorId = userId });
        context.Posts.Add(new Model.Entities.Post { Id = Guid.NewGuid(), Name = "P1", FractalId = fractalId, AuthorId = userId });
        await context.SaveChangesAsync();

        var fractals = await service.GetPublishedFractalsByUserAsync(userId, userId);

        Assert.Single(fractals);
    }

    [Fact]
    public async Task GetFractalByIdAsync_NotFound_ReturnsNull()
    {
        using var context = new ApplicationDbContext(GetDbContextOptions());
        var service = new FractalService(context);

        var result = await service.GetFractalByIdAsync(Guid.NewGuid(), Guid.NewGuid());
        Assert.Null(result);
    }

    [Fact]
    public async Task GetFractalByIdAsync_Success_ReturnsDto()
    {
        using var context = new ApplicationDbContext(GetDbContextOptions());
        var service = new FractalService(context);
        var fractalId = Guid.NewGuid();
        var currentUserId = Guid.NewGuid();

        context.Users.Add(new FractPalUser { Id = currentUserId, UserName = "user" });
        context.Fractals.Add(new Model.Entities.Fractal { Id = fractalId, Name = "F1", Axiom = "F", Rules = "F", Instructions = "F", Thumbnail = "T", AuthorId = currentUserId });
        await context.SaveChangesAsync();

        var result = await service.GetFractalByIdAsync(fractalId, currentUserId);

        Assert.NotNull(result);
        Assert.Equal("F1", result.Name);
    }

    [Fact]
    public async Task CreateFractalAsync_Success_ReturnsDto()
    {
        using var context = new ApplicationDbContext(GetDbContextOptions());
        var service = new FractalService(context);
        var userId = Guid.NewGuid();

        context.Users.Add(new FractPalUser { Id = userId, UserName = "user" });
        await context.SaveChangesAsync();

        var req = new CreateFractalRequest { Name = "F1", Axiom = "F", Rules = "F->FF", Generations = 2 };
        var result = await service.CreateFractalAsync(userId, req);

        Assert.NotNull(result);
        Assert.Equal("F1", result.Name);
        Assert.Equal("F", result.Axiom);
        Assert.Equal(userId.ToString(), result.UserId);
        Assert.Equal("user", result.Username);
    }

    [Fact]
    public async Task UpdateFractalAsync_NotFound_ThrowsKeyNotFoundException()
    {
        using var context = new ApplicationDbContext(GetDbContextOptions());
        var service = new FractalService(context);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.UpdateFractalAsync(Guid.NewGuid(), Guid.NewGuid(), new UpdateFractalRequest()));
    }

    [Fact]
    public async Task UpdateFractalAsync_NotOwner_ForksFractal()
    {
        using var context = new ApplicationDbContext(GetDbContextOptions());
        var service = new FractalService(context);
        var originalAuthorId = Guid.NewGuid();
        var newAuthorId = Guid.NewGuid();
        var fractalId = Guid.NewGuid();

        context.Users.Add(new FractPalUser { Id = newAuthorId, UserName = "newAuthor" });
        context.Users.Add(new FractPalUser { Id = originalAuthorId, UserName = "originalAuthor" });
        context.Fractals.Add(new Model.Entities.Fractal { Id = fractalId, Name = "Original", Axiom = "F", Rules = "F", Instructions = "F", Thumbnail = "T", AuthorId = originalAuthorId });
        context.Posts.Add(new Model.Entities.Post { Id = Guid.NewGuid(), Name = "P", FractalId = fractalId, AuthorId = originalAuthorId });
        await context.SaveChangesAsync();

        var req = new UpdateFractalRequest { Name = "Updated" };
        var result = await service.UpdateFractalAsync(fractalId, newAuthorId, req);

        Assert.NotNull(result);
        Assert.Equal("Updated (Copy)", result.Name);
        Assert.Equal(newAuthorId.ToString(), result.UserId);

        Assert.Equal(2, await context.Fractals.CountAsync());
    }

    [Fact]
    public async Task UpdateFractalAsync_IsOwner_UpdatesFractal()
    {
        using var context = new ApplicationDbContext(GetDbContextOptions());
        var service = new FractalService(context);
        var authorId = Guid.NewGuid();
        var fractalId = Guid.NewGuid();

        context.Users.Add(new FractPalUser { Id = authorId, UserName = "author" });
        context.Fractals.Add(new Model.Entities.Fractal { Id = fractalId, Name = "Original", Axiom = "F", Rules = "F", Instructions = "F", Thumbnail = "T", AuthorId = authorId });
        await context.SaveChangesAsync();

        var req = new UpdateFractalRequest { Name = "Updated" };
        var result = await service.UpdateFractalAsync(fractalId, authorId, req);

        Assert.NotNull(result);
        Assert.Equal("Updated", result.Name);

        var dbFractal = await context.Fractals.FindAsync(fractalId);
        Assert.Equal("Updated", dbFractal!.Name);
    }

    [Fact]
    public async Task DeleteFractalAsync_NotFound_ThrowsKeyNotFoundException()
    {
        using var context = new ApplicationDbContext(GetDbContextOptions());
        var service = new FractalService(context);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.DeleteFractalAsync(Guid.NewGuid(), Guid.NewGuid()));
    }

    [Fact]
    public async Task DeleteFractalAsync_NotOwner_ThrowsUnauthorizedAccessException()
    {
        using var context = new ApplicationDbContext(GetDbContextOptions());
        var service = new FractalService(context);
        var authorId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var fractalId = Guid.NewGuid();

        context.Fractals.Add(new Model.Entities.Fractal { Id = fractalId, Name = "F", Axiom = "F", Rules = "F", Instructions = "F", Thumbnail = "T", AuthorId = authorId });
        await context.SaveChangesAsync();

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.DeleteFractalAsync(fractalId, otherUserId));
    }

    [Fact]
    public async Task DeleteFractalAsync_Success_RemovesFractalAndDependencies()
    {
        using var context = new ApplicationDbContext(GetDbContextOptions());
        var service = new FractalService(context);
        var authorId = Guid.NewGuid();
        var fractalId = Guid.NewGuid();

        context.Fractals.Add(new Model.Entities.Fractal { Id = fractalId, Name = "F", Axiom = "F", Rules = "F", Instructions = "F", Thumbnail = "T", AuthorId = authorId });
        context.Likes.Add(new Model.Entities.Like { UserId = Guid.NewGuid(), FractalId = fractalId });
        context.Posts.Add(new Model.Entities.Post { Id = Guid.NewGuid(), FractalId = fractalId, Name = "P", Description = "D" });
        await context.SaveChangesAsync();

        await service.DeleteFractalAsync(fractalId, authorId);

        Assert.Null(await context.Fractals.FindAsync(fractalId));
        Assert.Empty(context.Likes);
        Assert.Empty(context.Posts);
    }

    [Fact]
    public async Task ForkFractalAsync_NotFound_ThrowsKeyNotFoundException()
    {
        using var context = new ApplicationDbContext(GetDbContextOptions());
        var service = new FractalService(context);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.ForkFractalAsync(Guid.NewGuid(), Guid.NewGuid()));
    }

    [Fact]
    public async Task ForkFractalAsync_Success_CreatesCopy()
    {
        using var context = new ApplicationDbContext(GetDbContextOptions());
        var service = new FractalService(context);
        var authorId = Guid.NewGuid();
        var forkerId = Guid.NewGuid();
        var fractalId = Guid.NewGuid();

        context.Users.Add(new FractPalUser { Id = forkerId, UserName = "forker" });
        context.Users.Add(new FractPalUser { Id = authorId, UserName = "author" });
        context.Fractals.Add(new Model.Entities.Fractal { Id = fractalId, Name = "Original", Axiom = "F", Rules = "F", Instructions = "F", Thumbnail = "T", AuthorId = authorId });
        context.Posts.Add(new Model.Entities.Post { Id = Guid.NewGuid(), Name = "P", FractalId = fractalId, AuthorId = authorId });
        await context.SaveChangesAsync();

        var result = await service.ForkFractalAsync(fractalId, forkerId);

        Assert.NotNull(result);
        Assert.Equal("Original (Copy)", result.Name);
        Assert.Equal(forkerId.ToString(), result.UserId);
        Assert.Equal(2, await context.Fractals.CountAsync());
    }
}