using System.Security.Claims;
using FractPal.API.Controllers;
using FractPal.Model.DTO.Comment;
using FractPal.Service.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace FractPal.Tests.Controllers;

public class CommentsControllerTests
{
    private readonly Mock<ICommentService> _commentServiceMock;
    private readonly CommentsController _controller;
    private readonly Guid _userId = Guid.NewGuid();

    public CommentsControllerTests()
    {
        _commentServiceMock = new Mock<ICommentService>();
        _controller = new CommentsController(_commentServiceMock.Object);

        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, _userId.ToString())
        }, "mock"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };
    }

    [Fact]
    public async Task GetCommentById_ValidId_ReturnsOk()
    {
        var commentId = Guid.NewGuid();
        var dto = new CommentDto { Id = commentId, Content = "Test" };
        _commentServiceMock.Setup(x => x.GetCommentById(commentId)).ReturnsAsync(dto);

        var result = await _controller.GetCommentById(commentId);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(dto, okResult.Value);
    }

    [Fact]
    public async Task GetCommentById_NotFound_ReturnsNotFound()
    {
        var commentId = Guid.NewGuid();
        _commentServiceMock.Setup(x => x.GetCommentById(commentId)).ThrowsAsync(new KeyNotFoundException());

        var result = await _controller.GetCommentById(commentId);

        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.NotNull(notFoundResult.Value);
    }

    [Fact]
    public async Task GetCommentById_Exception_Returns500()
    {
        var commentId = Guid.NewGuid();
        _commentServiceMock.Setup(x => x.GetCommentById(commentId)).ThrowsAsync(new Exception());

        var result = await _controller.GetCommentById(commentId);

        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusCodeResult.StatusCode);
    }

    [Fact]
    public async Task GetPostComments_ValidPostId_ReturnsOk()
    {
        var postId = Guid.NewGuid();
        var dtos = new List<CommentDto> { new CommentDto { Content = "Test" } };
        _commentServiceMock.Setup(x => x.GetPostComments(postId)).ReturnsAsync(dtos);

        var result = await _controller.GetPostComments(postId);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(dtos, okResult.Value);
    }

    [Fact]
    public async Task GetPostComments_Exception_Returns500()
    {
        var postId = Guid.NewGuid();
        _commentServiceMock.Setup(x => x.GetPostComments(postId)).ThrowsAsync(new Exception());

        var result = await _controller.GetPostComments(postId);

        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusCodeResult.StatusCode);
    }

    [Fact]
    public async Task CreateComment_InvalidModel_ReturnsBadRequest()
    {
        var postId = Guid.NewGuid();
        _controller.ModelState.AddModelError("Content", "Required");
        var result = await _controller.CreateComment(postId, new CreateCommentRequest());

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task CreateComment_ValidRequest_ReturnsCreatedAtAction()
    {
        var postId = Guid.NewGuid();
        var req = new CreateCommentRequest { Content = "Test" };
        var dto = new CommentDto { Id = Guid.NewGuid(), Content = "Test" };
        _commentServiceMock.Setup(x => x.CreateComment(_userId, postId, req)).ReturnsAsync(dto);

        var result = await _controller.CreateComment(postId, req);

        var createdAtResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(_controller.GetCommentById), createdAtResult.ActionName);
        Assert.Equal(dto.Id, createdAtResult.RouteValues?["id"]);
    }

    [Fact]
    public async Task CreateComment_PostNotFound_ReturnsNotFound()
    {
        var postId = Guid.NewGuid();
        var req = new CreateCommentRequest { Content = "Test" };
        _commentServiceMock.Setup(x => x.CreateComment(_userId, postId, req)).ThrowsAsync(new KeyNotFoundException());

        var result = await _controller.CreateComment(postId, req);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task CreateComment_Exception_Returns500()
    {
        var postId = Guid.NewGuid();
        var req = new CreateCommentRequest { Content = "Test" };
        _commentServiceMock.Setup(x => x.CreateComment(_userId, postId, req)).ThrowsAsync(new Exception());

        var result = await _controller.CreateComment(postId, req);

        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusCodeResult.StatusCode);
    }

    [Fact]
    public async Task UpdateComment_InvalidModel_ReturnsBadRequest()
    {
        var commentId = Guid.NewGuid();
        _controller.ModelState.AddModelError("Content", "Required");
        var result = await _controller.UpdateComment(commentId, new UpdateCommentRequest());

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task UpdateComment_ValidRequest_ReturnsOk()
    {
        var commentId = Guid.NewGuid();
        var req = new UpdateCommentRequest { Content = "Updated" };
        var dto = new CommentDto { Id = commentId, Content = "Updated" };
        _commentServiceMock.Setup(x => x.UpdateComment(_userId, commentId, req)).ReturnsAsync(dto);

        var result = await _controller.UpdateComment(commentId, req);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(dto, okResult.Value);
    }

    [Fact]
    public async Task UpdateComment_Unauthorized_ReturnsForbid()
    {
        var commentId = Guid.NewGuid();
        var req = new UpdateCommentRequest { Content = "Updated" };
        _commentServiceMock.Setup(x => x.UpdateComment(_userId, commentId, req)).ThrowsAsync(new UnauthorizedAccessException());

        var result = await _controller.UpdateComment(commentId, req);

        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task UpdateComment_NotFound_ReturnsNotFound()
    {
        var commentId = Guid.NewGuid();
        var req = new UpdateCommentRequest { Content = "Updated" };
        _commentServiceMock.Setup(x => x.UpdateComment(_userId, commentId, req)).ThrowsAsync(new KeyNotFoundException());

        var result = await _controller.UpdateComment(commentId, req);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task UpdateComment_Exception_Returns500()
    {
        var commentId = Guid.NewGuid();
        var req = new UpdateCommentRequest { Content = "Updated" };
        _commentServiceMock.Setup(x => x.UpdateComment(_userId, commentId, req)).ThrowsAsync(new Exception());

        var result = await _controller.UpdateComment(commentId, req);

        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusCodeResult.StatusCode);
    }

    [Fact]
    public async Task DeleteComment_ValidId_ReturnsNoContent()
    {
        var commentId = Guid.NewGuid();
        _commentServiceMock.Setup(x => x.DeleteComment(_userId, commentId)).Returns(Task.CompletedTask);

        var result = await _controller.DeleteComment(commentId);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task DeleteComment_NotFound_ReturnsNotFound()
    {
        var commentId = Guid.NewGuid();
        _commentServiceMock.Setup(x => x.DeleteComment(_userId, commentId)).ThrowsAsync(new KeyNotFoundException());

        var result = await _controller.DeleteComment(commentId);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task DeleteComment_Unauthorized_ReturnsForbid()
    {
        var commentId = Guid.NewGuid();
        _commentServiceMock.Setup(x => x.DeleteComment(_userId, commentId)).ThrowsAsync(new UnauthorizedAccessException());

        var result = await _controller.DeleteComment(commentId);

        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task DeleteComment_Exception_Returns500()
    {
        var commentId = Guid.NewGuid();
        _commentServiceMock.Setup(x => x.DeleteComment(_userId, commentId)).ThrowsAsync(new Exception());

        var result = await _controller.DeleteComment(commentId);

        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusCodeResult.StatusCode);
    }
}