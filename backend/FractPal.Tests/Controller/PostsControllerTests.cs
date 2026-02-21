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

}