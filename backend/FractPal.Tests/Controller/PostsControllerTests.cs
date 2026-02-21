using System.Security.Claims;
using FractPal.API.Controllers;
using FractPal.Model.DTO.Post;
using FractPal.Service.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace FractPal.Tests.Controllers;

public class PostsControllerTests
{
    private readonly Mock<IPostService> _postServiceMock;
    private readonly Mock<ILikeService> _likeServiceMock;
    private readonly PostsController _controller;
    private readonly Guid _userId = Guid.NewGuid();

    public PostsControllerTests()
    {
        _postServiceMock = new Mock<IPostService>();
        _likeServiceMock = new Mock<ILikeService>();
        _controller = new PostsController(_postServiceMock.Object, _likeServiceMock.Object);

        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, _userId.ToString()),
            new Claim(ClaimTypes.Role, "Admin")
        }, "mock"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };
    }

    [Fact]
    public async Task GetFeed_ReturnsOk()
    {
        var feedResponse = new PostFeedResponse();
        _postServiceMock.Setup(x => x.GetFeedAsync(_userId, 1, 20)).ReturnsAsync(feedResponse);

        var result = await _controller.GetFeed(1, 20);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(feedResponse, okResult.Value);
    }

    [Fact]
    public async Task GetFeed_Exception_Returns500()
    {
        _postServiceMock.Setup(x => x.GetFeedAsync(_userId, 1, 20)).ThrowsAsync(new Exception());

        var result = await _controller.GetFeed(1, 20);

        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusCodeResult.StatusCode);
    }

    [Fact]
    public async Task GetPostById_ReturnsOk()
    {
        var postId = Guid.NewGuid();
        var post = new PostDto { Id = postId };
        _postServiceMock.Setup(x => x.GetPostByIdAsync(postId, _userId)).ReturnsAsync(post);

        var result = await _controller.GetPostById(postId);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(post, okResult.Value);
    }

    [Fact]
    public async Task GetPostById_NotFound_ReturnsNotFound()
    {
        var postId = Guid.NewGuid();
        _postServiceMock.Setup(x => x.GetPostByIdAsync(postId, _userId)).ReturnsAsync((PostDto)null!);

        var result = await _controller.GetPostById(postId);

        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.NotNull(notFoundResult.Value);
    }

    [Fact]
    public async Task GetPostById_Exception_Returns500()
    {
        var postId = Guid.NewGuid();
        _postServiceMock.Setup(x => x.GetPostByIdAsync(postId, _userId)).ThrowsAsync(new Exception());

        var result = await _controller.GetPostById(postId);

        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusCodeResult.StatusCode);
    }

    [Fact]
    public async Task GetMyPosts_ReturnsOk()
    {
        var posts = new List<PostDto> { new PostDto() };
        _postServiceMock.Setup(x => x.GetUserPostsAsync(_userId, _userId)).ReturnsAsync(posts);

        var result = await _controller.GetMyPosts();

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(posts, okResult.Value);
    }

    [Fact]
    public async Task GetMyPosts_Exception_Returns500()
    {
        _postServiceMock.Setup(x => x.GetUserPostsAsync(_userId, _userId)).ThrowsAsync(new Exception());

        var result = await _controller.GetMyPosts();

        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusCodeResult.StatusCode);
    }

    [Fact]
    public async Task GetUserPosts_ReturnsOk()
    {
        var targetUserId = Guid.NewGuid();
        var posts = new List<PostDto> { new PostDto() };
        _postServiceMock.Setup(x => x.GetUserPostsAsync(targetUserId, _userId)).ReturnsAsync(posts);

        var result = await _controller.GetUserPosts(targetUserId);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(posts, okResult.Value);
    }

    [Fact]
    public async Task GetUserPosts_Exception_Returns500()
    {
        var targetUserId = Guid.NewGuid();
        _postServiceMock.Setup(x => x.GetUserPostsAsync(targetUserId, _userId)).ThrowsAsync(new Exception());

        var result = await _controller.GetUserPosts(targetUserId);

        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusCodeResult.StatusCode);
    }

    
    [Fact]
    public async Task PublishFractal_InvalidModel_ReturnsBadRequest()
    {
        var fractalId = Guid.NewGuid();
        _controller.ModelState.AddModelError("Title", "Required");
        var result = await _controller.PublishFractal(fractalId, new CreatePostRequest());

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task PublishFractal_ReturnsCreatedAtAction()
    {
        var fractalId = Guid.NewGuid();
        var req = new CreatePostRequest();
        var post = new PostDto { Id = Guid.NewGuid() };
        _postServiceMock.Setup(x => x.PublishFractalAsync(fractalId, _userId, req)).ReturnsAsync(post);

        var result = await _controller.PublishFractal(fractalId, req);

        var createdAtResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(_controller.GetPostById), createdAtResult.ActionName);
        Assert.Equal(post.Id, createdAtResult.RouteValues?["id"]);
    }

    [Fact]
    public async Task PublishFractal_NotFound_ReturnsNotFound()
    {
        var fractalId = Guid.NewGuid();
        var req = new CreatePostRequest();
        _postServiceMock.Setup(x => x.PublishFractalAsync(fractalId, _userId, req)).ThrowsAsync(new KeyNotFoundException());

        var result = await _controller.PublishFractal(fractalId, req);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task PublishFractal_Unauthorized_ReturnsForbid()
    {
        var fractalId = Guid.NewGuid();
        var req = new CreatePostRequest();
        _postServiceMock.Setup(x => x.PublishFractalAsync(fractalId, _userId, req)).ThrowsAsync(new UnauthorizedAccessException());

        var result = await _controller.PublishFractal(fractalId, req);

        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task PublishFractal_Conflict_ReturnsConflict()
    {
        var fractalId = Guid.NewGuid();
        var req = new CreatePostRequest();
        _postServiceMock.Setup(x => x.PublishFractalAsync(fractalId, _userId, req)).ThrowsAsync(new InvalidOperationException());

        var result = await _controller.PublishFractal(fractalId, req);

        Assert.IsType<ConflictResult>(result);
    }

    [Fact]
    public async Task PublishFractal_Exception_Returns500()
    {
        var fractalId = Guid.NewGuid();
        var req = new CreatePostRequest();
        _postServiceMock.Setup(x => x.PublishFractalAsync(fractalId, _userId, req)).ThrowsAsync(new Exception());

        var result = await _controller.PublishFractal(fractalId, req);

        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusCodeResult.StatusCode);
    }

    [Fact]
    public async Task UnpublishFractal_ReturnsNoContent()
    {
        var fractalId = Guid.NewGuid();
        _postServiceMock.Setup(x => x.UnpublishFractalAsync(fractalId, _userId)).Returns(Task.CompletedTask);

        var result = await _controller.UnpublishFractal(fractalId);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task UnpublishFractal_NotFound_ReturnsNotFound()
    {
        var fractalId = Guid.NewGuid();
        _postServiceMock.Setup(x => x.UnpublishFractalAsync(fractalId, _userId)).ThrowsAsync(new KeyNotFoundException());

        var result = await _controller.UnpublishFractal(fractalId);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task UnpublishFractal_Unauthorized_ReturnsForbid()
    {
        var fractalId = Guid.NewGuid();
        _postServiceMock.Setup(x => x.UnpublishFractalAsync(fractalId, _userId)).ThrowsAsync(new UnauthorizedAccessException());

        var result = await _controller.UnpublishFractal(fractalId);

        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task UnpublishFractal_Exception_Returns500()
    {
        var fractalId = Guid.NewGuid();
        _postServiceMock.Setup(x => x.UnpublishFractalAsync(fractalId, _userId)).ThrowsAsync(new Exception());

        var result = await _controller.UnpublishFractal(fractalId);

        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusCodeResult.StatusCode);
    }

    [Fact]
    public async Task UpdatePost_InvalidModel_ReturnsBadRequest()
    {
        var postId = Guid.NewGuid();
        _controller.ModelState.AddModelError("Content", "Required");
        var result = await _controller.UpdatePost(postId, new UpdatePostRequest());

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task UpdatePost_ReturnsOk()
    {
        var postId = Guid.NewGuid();
        var req = new UpdatePostRequest();
        var post = new PostDto { Id = postId };
        _postServiceMock.Setup(x => x.UpdatePostAsync(postId, _userId, req)).ReturnsAsync(post);

        var result = await _controller.UpdatePost(postId, req);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(post, okResult.Value);
    }

    [Fact]
    public async Task UpdatePost_NotFound_ReturnsNotFound()
    {
        var postId = Guid.NewGuid();
        var req = new UpdatePostRequest();
        _postServiceMock.Setup(x => x.UpdatePostAsync(postId, _userId, req)).ThrowsAsync(new KeyNotFoundException());

        var result = await _controller.UpdatePost(postId, req);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task UpdatePost_Unauthorized_ReturnsForbid()
    {
        var postId = Guid.NewGuid();
        var req = new UpdatePostRequest();
        _postServiceMock.Setup(x => x.UpdatePostAsync(postId, _userId, req)).ThrowsAsync(new UnauthorizedAccessException());

        var result = await _controller.UpdatePost(postId, req);

        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task UpdatePost_Exception_Returns500()
    {
        var postId = Guid.NewGuid();
        var req = new UpdatePostRequest();
        _postServiceMock.Setup(x => x.UpdatePostAsync(postId, _userId, req)).ThrowsAsync(new Exception());

        var result = await _controller.UpdatePost(postId, req);

        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusCodeResult.StatusCode);
    }

    [Fact]
    public async Task DeletePost_ReturnsNoContent()
    {
        var postId = Guid.NewGuid();
        _postServiceMock.Setup(x => x.DeletePostAsync(postId, _userId, true)).Returns(Task.CompletedTask);

        var result = await _controller.DeletePost(postId);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task DeletePost_NotFound_ReturnsNotFound()
    {
        var postId = Guid.NewGuid();
        _postServiceMock.Setup(x => x.DeletePostAsync(postId, _userId, true)).ThrowsAsync(new KeyNotFoundException());

        var result = await _controller.DeletePost(postId);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task DeletePost_Unauthorized_ReturnsForbid()
    {
        var postId = Guid.NewGuid();
        _postServiceMock.Setup(x => x.DeletePostAsync(postId, _userId, true)).ThrowsAsync(new UnauthorizedAccessException());

        var result = await _controller.DeletePost(postId);

        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task DeletePost_Exception_Returns500()
    {
        var postId = Guid.NewGuid();
        _postServiceMock.Setup(x => x.DeletePostAsync(postId, _userId, true)).ThrowsAsync(new Exception());

        var result = await _controller.DeletePost(postId);

        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusCodeResult.StatusCode);
    }

    [Fact]
    public async Task ToggleLikePost_ReturnsOk()
    {
        var postId = Guid.NewGuid();
        _likeServiceMock.Setup(x => x.ToggleLikePostAsync(postId, _userId)).ReturnsAsync(true);

        var result = await _controller.ToggleLikePost(postId);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task ToggleLikePost_NotFound_ReturnsNotFound()
    {
        var postId = Guid.NewGuid();
        _likeServiceMock.Setup(x => x.ToggleLikePostAsync(postId, _userId)).ThrowsAsync(new KeyNotFoundException());

        var result = await _controller.ToggleLikePost(postId);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task ToggleLikePost_Exception_Returns500()
    {
        var postId = Guid.NewGuid();
        _likeServiceMock.Setup(x => x.ToggleLikePostAsync(postId, _userId)).ThrowsAsync(new Exception());

        var result = await _controller.ToggleLikePost(postId);

        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusCodeResult.StatusCode);
    }

}