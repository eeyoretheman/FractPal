using FractPal.Data;
using FractPal.Model.DTO.Comment;
using FractPal.Model.Entities;
using FractPal.Service.Implementation;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FractPal.Tests.Services;

public class CommentServiceTests
{
    private DbContextOptions<ApplicationDbContext> GetDbContextOptions()
    {
        return new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    [Fact]
    public async Task CreateComment_PostNotFound_ThrowsKeyNotFoundException()
    {
        using var context = new ApplicationDbContext(GetDbContextOptions());
        var service = new CommentService(context);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.CreateComment(Guid.NewGuid(), Guid.NewGuid(), new CreateCommentRequest()));
    }

    [Fact]
    public async Task CreateComment_Success_ReturnsDto()
    {
        using var context = new ApplicationDbContext(GetDbContextOptions());
        var service = new CommentService(context);

        var userId = Guid.NewGuid();
        var postId = Guid.NewGuid();

        context.Users.Add(new FractPalUser { Id = userId, UserName = "user" });
        context.Posts.Add(new Model.Entities.Post { Id = postId, Name = "P", AuthorId = userId });
        await context.SaveChangesAsync();

        var request = new CreateCommentRequest { Content = "Test content" };
        var result = await service.CreateComment(userId, postId, request);

        Assert.NotNull(result);
        Assert.Equal("Test content", result.Content);
        Assert.Equal(postId, result.PostId);
        Assert.Equal(userId, result.AuthorId);
        Assert.Equal("user", result.Username);
    }

    [Fact]
    public async Task DeleteComment_CommentNotFound_ThrowsKeyNotFoundException()
    {
        using var context = new ApplicationDbContext(GetDbContextOptions());
        var service = new CommentService(context);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.DeleteComment(Guid.NewGuid(), Guid.NewGuid()));
    }

    [Fact]
    public async Task DeleteComment_Unauthorized_ThrowsUnauthorizedAccessException()
    {
        using var context = new ApplicationDbContext(GetDbContextOptions());
        var service = new CommentService(context);

        var commentId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        context.Comments.Add(new Model.Entities.Comment { Id = commentId, Content = "C", AuthorId = authorId, PostId = Guid.NewGuid() });
        await context.SaveChangesAsync();

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.DeleteComment(otherUserId, commentId));
    }

    [Fact]
    public async Task DeleteComment_Success_RemovesComment()
    {
        using var context = new ApplicationDbContext(GetDbContextOptions());
        var service = new CommentService(context);

        var commentId = Guid.NewGuid();
        var authorId = Guid.NewGuid();

        context.Comments.Add(new Model.Entities.Comment { Id = commentId, Content = "C", AuthorId = authorId, PostId = Guid.NewGuid() });
        await context.SaveChangesAsync();

        await service.DeleteComment(authorId, commentId);

        var comment = await context.Comments.FindAsync(commentId);
        Assert.Null(comment);
    }

    [Fact]
    public async Task GetCommentById_NotFound_ThrowsKeyNotFoundException()
    {
        using var context = new ApplicationDbContext(GetDbContextOptions());
        var service = new CommentService(context);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.GetCommentById(Guid.NewGuid()));
    }

    [Fact]
    public async Task GetCommentById_Success_ReturnsDto()
    {
        using var context = new ApplicationDbContext(GetDbContextOptions());
        var service = new CommentService(context);

        var commentId = Guid.NewGuid();
        var authorId = Guid.NewGuid();

        context.Users.Add(new FractPalUser { Id = authorId, UserName = "author" });
        context.Comments.Add(new Model.Entities.Comment { Id = commentId, AuthorId = authorId, PostId = Guid.NewGuid(), Content = "Test" });
        await context.SaveChangesAsync();

        var result = await service.GetCommentById(commentId);

        Assert.NotNull(result);
        Assert.Equal(commentId, result.Id);
        Assert.Equal("Test", result.Content);
        Assert.Equal("author", result.Username);
    }

    [Fact]
    public async Task GetPostComments_ReturnsList()
    {
        using var context = new ApplicationDbContext(GetDbContextOptions());
        var service = new CommentService(context);

        var postId = Guid.NewGuid();
        var authorId = Guid.NewGuid();

        context.Users.Add(new FractPalUser { Id = authorId, UserName = "author" });
        context.Comments.Add(new Model.Entities.Comment { Id = Guid.NewGuid(), AuthorId = authorId, PostId = postId, Content = "Test 1", CreatedAt = DateTime.UtcNow });
        context.Comments.Add(new Model.Entities.Comment { Id = Guid.NewGuid(), AuthorId = authorId, PostId = postId, Content = "Test 2", CreatedAt = DateTime.UtcNow.AddMinutes(1) });
        await context.SaveChangesAsync();

        var result = await service.GetPostComments(postId);

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal("Test 1", result[0].Content);
        Assert.Equal("Test 2", result[1].Content);
    }

    [Fact]
    public async Task UpdateComment_NotFound_ThrowsKeyNotFoundException()
    {
        using var context = new ApplicationDbContext(GetDbContextOptions());
        var service = new CommentService(context);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.UpdateComment(Guid.NewGuid(), Guid.NewGuid(), new UpdateCommentRequest()));
    }

    [Fact]
    public async Task UpdateComment_Unauthorized_ThrowsUnauthorizedAccessException()
    {
        using var context = new ApplicationDbContext(GetDbContextOptions());
        var service = new CommentService(context);

        var commentId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        context.Comments.Add(new Model.Entities.Comment { Id = commentId, Content = "C", AuthorId = authorId, PostId = Guid.NewGuid() });
        await context.SaveChangesAsync();

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.UpdateComment(otherUserId, commentId, new UpdateCommentRequest()));
    }

    [Fact]
    public async Task UpdateComment_Success_ReturnsUpdatedDto()
    {
        using var context = new ApplicationDbContext(GetDbContextOptions());
        var service = new CommentService(context);

        var commentId = Guid.NewGuid();
        var authorId = Guid.NewGuid();

        context.Users.Add(new FractPalUser { Id = authorId, UserName = "author" });
        context.Comments.Add(new Model.Entities.Comment { Id = commentId, AuthorId = authorId, PostId = Guid.NewGuid(), Content = "Old" });
        await context.SaveChangesAsync();

        var request = new UpdateCommentRequest { Content = "New" };
        var result = await service.UpdateComment(authorId, commentId, request);

        Assert.NotNull(result);
        Assert.Equal("New", result.Content);
        Assert.NotNull(result.UpdatedAt);
    }
}