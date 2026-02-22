using FractPal.Data;
using FractPal.Model.Entities;
using FractPal.Service.Implementation;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FractPal.Tests.Services;

public class LikeServiceTests
{
    private DbContextOptions<ApplicationDbContext> GetDbContextOptions()
    {
        return new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    [Fact]
    public async Task ToggleLikeAsync_FractalNotFound_ThrowsKeyNotFoundException()
    {
        using var context = new ApplicationDbContext(GetDbContextOptions());
        var service = new LikeService(context);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.ToggleLikeAsync(Guid.NewGuid(), Guid.NewGuid()));
    }

    [Fact]
    public async Task ToggleLikeAsync_NewLike_ReturnsTrue()
    {
        using var context = new ApplicationDbContext(GetDbContextOptions());
        var service = new LikeService(context);
        var fractalId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        context.Fractals.Add(new Model.Entities.Fractal { Id = fractalId, Name = "F", Axiom = "F", Rules = "F", Instructions = "F", Thumbnail = "T", AuthorId = Guid.NewGuid() });
        await context.SaveChangesAsync();

        var result = await service.ToggleLikeAsync(fractalId, userId);

        Assert.True(result);
        var like = await context.Likes.FirstOrDefaultAsync(l => l.FractalId == fractalId && l.UserId == userId);
        Assert.NotNull(like);
    }

    [Fact]
    public async Task ToggleLikeAsync_ExistingLike_RemovesLikeReturnsFalse()
    {
        using var context = new ApplicationDbContext(GetDbContextOptions());
        var service = new LikeService(context);
        var fractalId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        context.Fractals.Add(new Model.Entities.Fractal { Id = fractalId, Name = "F", Axiom = "F", Rules = "F", Instructions = "F", Thumbnail = "T", AuthorId = Guid.NewGuid() });
        context.Likes.Add(new Model.Entities.Like { FractalId = fractalId, UserId = userId });
        await context.SaveChangesAsync();

        var result = await service.ToggleLikeAsync(fractalId, userId);

        Assert.False(result);
        var like = await context.Likes.FirstOrDefaultAsync(l => l.FractalId == fractalId && l.UserId == userId);
        Assert.Null(like);
    }

    [Fact]
    public async Task ToggleLikePostAsync_PostNotFound_ThrowsKeyNotFoundException()
    {
        using var context = new ApplicationDbContext(GetDbContextOptions());
        var service = new LikeService(context);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.ToggleLikePostAsync(Guid.NewGuid(), Guid.NewGuid()));
    }

    [Fact]
    public async Task ToggleLikePostAsync_Success_ReturnsTrue()
    {
        using var context = new ApplicationDbContext(GetDbContextOptions());
        var service = new LikeService(context);
        var postId = Guid.NewGuid();
        var fractalId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        context.Fractals.Add(new Model.Entities.Fractal { Id = fractalId, Name = "F", Axiom = "F", Rules = "F", Instructions = "F", Thumbnail = "T", AuthorId = Guid.NewGuid() });
        context.Posts.Add(new Model.Entities.Post { Id = postId, Name = "P", FractalId = fractalId, AuthorId = Guid.NewGuid() });
        await context.SaveChangesAsync();

        var result = await service.ToggleLikePostAsync(postId, userId);

        Assert.True(result);
        var like = await context.Likes.FirstOrDefaultAsync(l => l.FractalId == fractalId && l.UserId == userId);
        Assert.NotNull(like);
    }
}
