using FractPal.Data;
using FractPal.Model.DTO.Post;
using FractPal.Model.Entities;
using FractPal.Service.Implementation;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FractPal.Tests.Services;

public class PostServiceTests
{
    private DbContextOptions<ApplicationDbContext> GetDbContextOptions()
    {
        return new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    [Fact]
    public async Task GetFeedAsync_ReturnsPaginatedPosts()
    {
        using var context = new ApplicationDbContext(GetDbContextOptions());
        var service = new PostService(context);
        var userId = Guid.NewGuid();

        context.Users.Add(new FractPalUser { Id = userId, UserName = "user" });
        var fractalId1 = Guid.NewGuid();
        context.Fractals.Add(new Model.Entities.Fractal { Id = fractalId1, Name = "F1", Axiom = "F", Rules = "F", Instructions = "F", Thumbnail = "T", AuthorId = userId });
        context.Posts.Add(new Model.Entities.Post { Id = Guid.NewGuid(), Name = "P1", CreatedAt = DateTime.UtcNow, FractalId = fractalId1, AuthorId = userId });

        var fractalId2 = Guid.NewGuid();
        context.Fractals.Add(new Model.Entities.Fractal { Id = fractalId2, Name = "F2", Axiom = "F", Rules = "F", Instructions = "F", Thumbnail = "T", AuthorId = userId });
        context.Posts.Add(new Model.Entities.Post { Id = Guid.NewGuid(), Name = "P2", CreatedAt = DateTime.UtcNow.AddMinutes(-5), FractalId = fractalId2, AuthorId = userId });
        await context.SaveChangesAsync();

        var result = await service.GetFeedAsync(userId, 1, 1);

        Assert.NotNull(result);
        Assert.Equal(2, result.TotalCount);
        Assert.Single(result.Posts);
        Assert.Equal("P1", result.Posts[0].Name);
    }

    [Fact]
    public async Task GetPostByIdAsync_NotFound_ReturnsNull()
    {
        using var context = new ApplicationDbContext(GetDbContextOptions());
        var service = new PostService(context);

        var result = await service.GetPostByIdAsync(Guid.NewGuid(), Guid.NewGuid());
        Assert.Null(result);
    }

    [Fact]
    public async Task GetPostByIdAsync_Success_ReturnsDto()
    {
        using var context = new ApplicationDbContext(GetDbContextOptions());
        var service = new PostService(context);
        var postId = Guid.NewGuid();

        var authorId = Guid.NewGuid();
        context.Users.Add(new FractPalUser { Id = authorId, UserName = "user" });
        var fractalId = Guid.NewGuid();
        context.Fractals.Add(new Model.Entities.Fractal { Id = fractalId, Name = "F1", Axiom = "F", Rules = "F", Instructions = "F", Thumbnail = "T", AuthorId = authorId });
        context.Posts.Add(new Model.Entities.Post { Id = postId, Name = "P1", FractalId = fractalId, AuthorId = authorId });
        await context.SaveChangesAsync();

        var result = await service.GetPostByIdAsync(postId, Guid.NewGuid());

        Assert.NotNull(result);
        Assert.Equal("P1", result.Name);
    }

    [Fact]
    public async Task GetUserPostsAsync_ReturnsUserPosts()
    {
        using var context = new ApplicationDbContext(GetDbContextOptions());
        var service = new PostService(context);
        var authorId = Guid.NewGuid();

        context.Users.Add(new FractPalUser { Id = authorId, UserName = "user" });
        var otherUserId = Guid.NewGuid();
        context.Users.Add(new FractPalUser { Id = otherUserId, UserName = "other" });

        var fractalId1 = Guid.NewGuid();
        context.Fractals.Add(new Model.Entities.Fractal { Id = fractalId1, Name = "F1", Axiom = "F", Rules = "F", Instructions = "F", Thumbnail = "T", AuthorId = authorId });
        context.Posts.Add(new Model.Entities.Post { Id = Guid.NewGuid(), Name = "P1", AuthorId = authorId, FractalId = fractalId1 });

        var fractalId2 = Guid.NewGuid();
        context.Fractals.Add(new Model.Entities.Fractal { Id = fractalId2, Name = "F2", Axiom = "F", Rules = "F", Instructions = "F", Thumbnail = "T", AuthorId = otherUserId });
        context.Posts.Add(new Model.Entities.Post { Id = Guid.NewGuid(), Name = "P2", AuthorId = otherUserId, FractalId = fractalId2 });
        await context.SaveChangesAsync();

        var result = await service.GetUserPostsAsync(authorId, authorId);

        Assert.Single(result);
        Assert.Equal("P1", result[0].Name);
    }

    [Fact]
    public async Task PublishFractalAsync_FractalNotFound_ThrowsKeyNotFoundException()
    {
        using var context = new ApplicationDbContext(GetDbContextOptions());
        var service = new PostService(context);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.PublishFractalAsync(Guid.NewGuid(), Guid.NewGuid(), new CreatePostRequest()));
    }

    [Fact]
    public async Task PublishFractalAsync_NotAuthor_ThrowsUnauthorizedAccessException()
    {
        using var context = new ApplicationDbContext(GetDbContextOptions());
        var service = new PostService(context);
        var fractalId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        context.Fractals.Add(new Model.Entities.Fractal { Id = fractalId, Name = "F", Axiom = "F", Rules = "F", Instructions = "F", Thumbnail = "T", AuthorId = authorId });
        await context.SaveChangesAsync();

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.PublishFractalAsync(fractalId, otherUserId, new CreatePostRequest()));
    }

    [Fact]
    public async Task PublishFractalAsync_AlreadyPublished_ThrowsInvalidOperationException()
    {
        using var context = new ApplicationDbContext(GetDbContextOptions());
        var service = new PostService(context);
        var fractalId = Guid.NewGuid();
        var authorId = Guid.NewGuid();

        context.Fractals.Add(new Model.Entities.Fractal { Id = fractalId, Name = "F", Axiom = "F", Rules = "F", Instructions = "F", Thumbnail = "T", AuthorId = authorId });
        context.Posts.Add(new Model.Entities.Post { Id = Guid.NewGuid(), Name = "P", FractalId = fractalId, AuthorId = authorId });
        await context.SaveChangesAsync();

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.PublishFractalAsync(fractalId, authorId, new CreatePostRequest()));
    }

    [Fact]
    public async Task PublishFractalAsync_Success_ReturnsDto()
    {
        using var context = new ApplicationDbContext(GetDbContextOptions());
        var service = new PostService(context);
        var fractalId = Guid.NewGuid();
        var authorId = Guid.NewGuid();

        context.Users.Add(new FractPalUser { Id = authorId, UserName = "author" });
        context.Fractals.Add(new Model.Entities.Fractal { Id = fractalId, Name = "F", Axiom = "F", Rules = "F", Instructions = "F", Thumbnail = "T", AuthorId = authorId });
        await context.SaveChangesAsync();

        var req = new CreatePostRequest { Name = "Post", Description = "Desc" };
        var result = await service.PublishFractalAsync(fractalId, authorId, req);

        Assert.NotNull(result);
        Assert.Equal("Post", result.Name);
        Assert.Equal("Desc", result.Description);
        Assert.Equal("author", result.Username);
        Assert.Single(context.Posts);
    }

    [Fact]
    public async Task UnpublishFractalAsync_PostNotFound_ThrowsKeyNotFoundException()
    {
        using var context = new ApplicationDbContext(GetDbContextOptions());
        var service = new PostService(context);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.UnpublishFractalAsync(Guid.NewGuid(), Guid.NewGuid()));
    }

    [Fact]
    public async Task UnpublishFractalAsync_Success_RemovesPost()
    {
        using var context = new ApplicationDbContext(GetDbContextOptions());
        var service = new PostService(context);
        var fractalId = Guid.NewGuid();
        var authorId = Guid.NewGuid();

        context.Posts.Add(new Model.Entities.Post { Id = Guid.NewGuid(), Name = "P", FractalId = fractalId, AuthorId = authorId });
        context.Likes.Add(new Model.Entities.Like { UserId = Guid.NewGuid(), FractalId = fractalId });
        await context.SaveChangesAsync();

        await service.UnpublishFractalAsync(fractalId, authorId);

        Assert.Empty(context.Posts);
        Assert.Empty(context.Likes);
    }

    [Fact]
    public async Task UpdatePostAsync_NotFound_ThrowsKeyNotFoundException()
    {
        using var context = new ApplicationDbContext(GetDbContextOptions());
        var service = new PostService(context);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.UpdatePostAsync(Guid.NewGuid(), Guid.NewGuid(), new UpdatePostRequest()));
    }

    [Fact]
    public async Task UpdatePostAsync_NotAuthor_ThrowsUnauthorizedAccessException()
    {
        using var context = new ApplicationDbContext(GetDbContextOptions());
        var service = new PostService(context);
        var postId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        context.Posts.Add(new Model.Entities.Post { Id = postId, Name = "P", AuthorId = authorId });
        await context.SaveChangesAsync();

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.UpdatePostAsync(postId, otherUserId, new UpdatePostRequest()));
    }

    [Fact]
    public async Task UpdatePostAsync_Success_ReturnsDto()
    {
        using var context = new ApplicationDbContext(GetDbContextOptions());
        var service = new PostService(context);
        var postId = Guid.NewGuid();
        var authorId = Guid.NewGuid();

        context.Users.Add(new FractPalUser { Id = authorId, UserName = "author" });
        context.Posts.Add(new Model.Entities.Post { Id = postId, AuthorId = authorId, Name = "Old" });
        await context.SaveChangesAsync();

        var req = new UpdatePostRequest { Name = "New", Description = "Desc" };
        var result = await service.UpdatePostAsync(postId, authorId, req);

        Assert.NotNull(result);
        Assert.Equal("New", result.Name);
        Assert.Equal("Desc", result.Description);
        Assert.Equal("author", result.Username);
    }

    [Fact]
    public async Task DeletePostAsync_NotFound_ThrowsKeyNotFoundException()
    {
        using var context = new ApplicationDbContext(GetDbContextOptions());
        var service = new PostService(context);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.DeletePostAsync(Guid.NewGuid(), Guid.NewGuid()));
    }

    [Fact]
    public async Task DeletePostAsync_NotAuthorNotAdmin_ThrowsUnauthorizedAccessException()
    {
        using var context = new ApplicationDbContext(GetDbContextOptions());
        var service = new PostService(context);
        var postId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        context.Posts.Add(new Model.Entities.Post { Id = postId, Name = "P", AuthorId = authorId });
        await context.SaveChangesAsync();

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.DeletePostAsync(postId, otherUserId));
    }

    [Fact]
    public async Task DeletePostAsync_SuccessAsAuthor_RemovesPost()
    {
        using var context = new ApplicationDbContext(GetDbContextOptions());
        var service = new PostService(context);
        var postId = Guid.NewGuid();
        var authorId = Guid.NewGuid();

        context.Posts.Add(new Model.Entities.Post { Id = postId, Name = "P", AuthorId = authorId, FractalId = Guid.NewGuid() });
        await context.SaveChangesAsync();

        await service.DeletePostAsync(postId, authorId);

        Assert.Empty(context.Posts);
    }

    [Fact]
    public async Task DeletePostAsync_SuccessAsAdmin_RemovesPost()
    {
        using var context = new ApplicationDbContext(GetDbContextOptions());
        var service = new PostService(context);
        var postId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var adminId = Guid.NewGuid();

        context.Posts.Add(new Model.Entities.Post { Id = postId, Name = "P", AuthorId = authorId, FractalId = Guid.NewGuid() });
        await context.SaveChangesAsync();

        await service.DeletePostAsync(postId, adminId, isAdmin: true);

        Assert.Empty(context.Posts);
    }
}
