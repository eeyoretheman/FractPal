using System.Security.Claims;
using FractPal.API.Controllers;
using FractPal.Model.DTO.Fractal;
using FractPal.Service.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace FractPal.Tests.Controllers;

public class FractalControllerTests
{
    private readonly Mock<IFractalService> _fractalServiceMock;
    private readonly Mock<ILikeService> _likeServiceMock;
    private readonly FractalController _controller;
    private readonly Guid _userId = Guid.NewGuid();

    public FractalControllerTests()
    {
        _fractalServiceMock = new Mock<IFractalService>();
        _likeServiceMock = new Mock<ILikeService>();
        _controller = new FractalController(_fractalServiceMock.Object, _likeServiceMock.Object);

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
    public async Task GetFeed_ReturnsOk()
    {
        var feedResponse = new FractalFeedResponse();
        _fractalServiceMock.Setup(x => x.GetFeedAsync(_userId, 1, 20)).ReturnsAsync(feedResponse);

        var result = await _controller.GetFeed(1, 20);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(feedResponse, okResult.Value);
    }

    [Fact]
    public async Task GetFeed_Exception_Returns500()
    {
        _fractalServiceMock.Setup(x => x.GetFeedAsync(_userId, 1, 20)).ThrowsAsync(new Exception());

        var result = await _controller.GetFeed(1, 20);

        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusCodeResult.StatusCode);
    }

    [Fact]
    public async Task GetMyFractals_ReturnsOk()
    {
        var fractals = new List<FractalDto> { new FractalDto() };
        _fractalServiceMock.Setup(x => x.GetUserFractalsAsync(_userId, _userId)).ReturnsAsync(fractals);

        var result = await _controller.GetMyFractals();

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(fractals, okResult.Value);
    }

    [Fact]
    public async Task GetMyFractals_Exception_Returns500()
    {
        _fractalServiceMock.Setup(x => x.GetUserFractalsAsync(_userId, _userId)).ThrowsAsync(new Exception());

        var result = await _controller.GetMyFractals();

        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusCodeResult.StatusCode);
    }

    [Fact]
    public async Task GetUserFractals_ReturnsOk()
    {
        var targetUserId = Guid.NewGuid();
        var fractals = new List<FractalDto> { new FractalDto() };
        _fractalServiceMock.Setup(x => x.GetPublishedFractalsByUserAsync(targetUserId, _userId)).ReturnsAsync(fractals);

        var result = await _controller.GetUserFractals(targetUserId);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(fractals, okResult.Value);
    }

    [Fact]
    public async Task GetUserFractals_Exception_Returns500()
    {
        var targetUserId = Guid.NewGuid();
        _fractalServiceMock.Setup(x => x.GetPublishedFractalsByUserAsync(targetUserId, _userId)).ThrowsAsync(new Exception());

        var result = await _controller.GetUserFractals(targetUserId);

        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusCodeResult.StatusCode);
    }

    [Fact]
    public async Task GetFractalById_ReturnsOk()
    {
        var fractalId = Guid.NewGuid();
        var fractal = new FractalDto { Id = fractalId.ToString() };
        _fractalServiceMock.Setup(x => x.GetFractalByIdAsync(fractalId, _userId)).ReturnsAsync(fractal);

        var result = await _controller.GetFractalById(fractalId);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(fractal, okResult.Value);
    }

    [Fact]
    public async Task GetFractalById_NotFound_ReturnsNotFound()
    {
        var fractalId = Guid.NewGuid();
        _fractalServiceMock.Setup(x => x.GetFractalByIdAsync(fractalId, _userId)).ReturnsAsync((FractalDto)null!);

        var result = await _controller.GetFractalById(fractalId);

        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.NotNull(notFoundResult.Value);
    }

    [Fact]
    public async Task GetFractalById_Exception_Returns500()
    {
        var fractalId = Guid.NewGuid();
        _fractalServiceMock.Setup(x => x.GetFractalByIdAsync(fractalId, _userId)).ThrowsAsync(new Exception());

        var result = await _controller.GetFractalById(fractalId);

        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusCodeResult.StatusCode);
    }

    [Fact]
    public async Task CreateFractal_InvalidModel_ReturnsBadRequest()
    {
        _controller.ModelState.AddModelError("Name", "Required");
        var result = await _controller.CreateFractal(new CreateFractalRequest());

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task CreateFractal_ReturnsCreatedAtAction()
    {
        var req = new CreateFractalRequest();
        var fractalId = Guid.NewGuid();
        var fractal = new FractalDto { Id = fractalId.ToString() };
        _fractalServiceMock.Setup(x => x.CreateFractalAsync(_userId, req)).ReturnsAsync(fractal);

        var result = await _controller.CreateFractal(req);

        var createdAtResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(_controller.GetFractalById), createdAtResult.ActionName);
        Assert.Equal(fractal.Id, createdAtResult.RouteValues?["id"]?.ToString());
    }

    [Fact]
    public async Task CreateFractal_Exception_Returns500()
    {
        var req = new CreateFractalRequest();
        _fractalServiceMock.Setup(x => x.CreateFractalAsync(_userId, req)).ThrowsAsync(new Exception());

        var result = await _controller.CreateFractal(req);

        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusCodeResult.StatusCode);
    }

    [Fact]
    public async Task UpdateFractal_InvalidModel_ReturnsBadRequest()
    {
        var fractalId = Guid.NewGuid();
        _controller.ModelState.AddModelError("Name", "Required");
        var result = await _controller.UpdateFractal(fractalId, new UpdateFractalRequest());

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task UpdateFractal_ReturnsOk()
    {
        var fractalId = Guid.NewGuid();
        var req = new UpdateFractalRequest();
        var fractal = new FractalDto { Id = fractalId.ToString() };
        _fractalServiceMock.Setup(x => x.UpdateFractalAsync(fractalId, _userId, req)).ReturnsAsync(fractal);

        var result = await _controller.UpdateFractal(fractalId, req);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(fractal, okResult.Value);
    }

    [Fact]
    public async Task UpdateFractal_NotFound_ReturnsNotFound()
    {
        var fractalId = Guid.NewGuid();
        var req = new UpdateFractalRequest();
        _fractalServiceMock.Setup(x => x.UpdateFractalAsync(fractalId, _userId, req)).ThrowsAsync(new KeyNotFoundException());

        var result = await _controller.UpdateFractal(fractalId, req);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task UpdateFractal_Unauthorized_ReturnsForbid()
    {
        var fractalId = Guid.NewGuid();
        var req = new UpdateFractalRequest();
        _fractalServiceMock.Setup(x => x.UpdateFractalAsync(fractalId, _userId, req)).ThrowsAsync(new UnauthorizedAccessException());

        var result = await _controller.UpdateFractal(fractalId, req);

        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task UpdateFractal_Exception_Returns500()
    {
        var fractalId = Guid.NewGuid();
        var req = new UpdateFractalRequest();
        _fractalServiceMock.Setup(x => x.UpdateFractalAsync(fractalId, _userId, req)).ThrowsAsync(new Exception());

        var result = await _controller.UpdateFractal(fractalId, req);

        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusCodeResult.StatusCode);
    }

    [Fact]
    public async Task DeleteFractal_ReturnsNoContent()
    {
        var fractalId = Guid.NewGuid();
        _fractalServiceMock.Setup(x => x.DeleteFractalAsync(fractalId, _userId)).Returns(Task.CompletedTask);

        var result = await _controller.DeleteFractal(fractalId);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task DeleteFractal_NotFound_ReturnsNotFound()
    {
        var fractalId = Guid.NewGuid();
        _fractalServiceMock.Setup(x => x.DeleteFractalAsync(fractalId, _userId)).ThrowsAsync(new KeyNotFoundException());

        var result = await _controller.DeleteFractal(fractalId);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task DeleteFractal_Unauthorized_ReturnsForbid()
    {
        var fractalId = Guid.NewGuid();
        _fractalServiceMock.Setup(x => x.DeleteFractalAsync(fractalId, _userId)).ThrowsAsync(new UnauthorizedAccessException());

        var result = await _controller.DeleteFractal(fractalId);

        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task DeleteFractal_Exception_Returns500()
    {
        var fractalId = Guid.NewGuid();
        _fractalServiceMock.Setup(x => x.DeleteFractalAsync(fractalId, _userId)).ThrowsAsync(new Exception());

        var result = await _controller.DeleteFractal(fractalId);

        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusCodeResult.StatusCode);
    }

    [Fact]
    public async Task ForkFractal_ReturnsCreatedAtAction()
    {
        var fractalId = Guid.NewGuid();
        var fractal = new FractalDto { Id = "fork" };
        _fractalServiceMock.Setup(x => x.ForkFractalAsync(fractalId, _userId)).ReturnsAsync(fractal);

        var result = await _controller.ForkFractal(fractalId);

        var createdAtResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(_controller.GetFractalById), createdAtResult.ActionName);
        Assert.Equal(fractal.Id, createdAtResult.RouteValues?["id"]?.ToString());
    }

    [Fact]
    public async Task ForkFractal_NotFound_ReturnsNotFound()
    {
        var fractalId = Guid.NewGuid();
        _fractalServiceMock.Setup(x => x.ForkFractalAsync(fractalId, _userId)).ThrowsAsync(new KeyNotFoundException());

        var result = await _controller.ForkFractal(fractalId);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task ForkFractal_Exception_Returns500()
    {
        var fractalId = Guid.NewGuid();
        _fractalServiceMock.Setup(x => x.ForkFractalAsync(fractalId, _userId)).ThrowsAsync(new Exception());

        var result = await _controller.ForkFractal(fractalId);

        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusCodeResult.StatusCode);
    }
}