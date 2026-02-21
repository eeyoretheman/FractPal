using System.Security.Claims;
using FractPal.API.Controllers;
using FractPal.Model.DTO.User;
using FractPal.Service.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace FractPal.Tests.Controllers;

public class UserControllerTests
{
    private readonly Mock<IUserService> _userServiceMock;
    private readonly UserController _controller;
    private readonly string _userId = Guid.NewGuid().ToString();

    public UserControllerTests()
    {
        _userServiceMock = new Mock<IUserService>();
        _controller = new UserController(_userServiceMock.Object);

        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, _userId)
        }, "mock"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };
    }

    [Fact]
    public async Task GetMyProfile_ReturnsOk()
    {
        var profile = new UserProfileDto();
        _userServiceMock.Setup(x => x.GetProfileAsync(_userId, _userId)).ReturnsAsync(profile);

        var result = await _controller.GetMyProfile();

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(profile, okResult.Value);
    }

    [Fact]
    public async Task GetMyProfile_NotFound_ReturnsNotFound()
    {
        _userServiceMock.Setup(x => x.GetProfileAsync(_userId, _userId)).ThrowsAsync(new KeyNotFoundException());

        var result = await _controller.GetMyProfile();

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task GetMyProfile_Exception_Returns500()
    {
        _userServiceMock.Setup(x => x.GetProfileAsync(_userId, _userId)).ThrowsAsync(new Exception());

        var result = await _controller.GetMyProfile();

        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusCodeResult.StatusCode);
    }

    [Fact]
    public async Task GetUserProfile_ReturnsOk()
    {
        var targetId = Guid.NewGuid().ToString();
        var profile = new UserProfileDto();
        _userServiceMock.Setup(x => x.GetProfileAsync(targetId, _userId)).ReturnsAsync(profile);

        var result = await _controller.GetUserProfile(targetId);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(profile, okResult.Value);
    }

    [Fact]
    public async Task GetUserProfile_NotFound_ReturnsNotFound()
    {
        var targetId = Guid.NewGuid().ToString();
        _userServiceMock.Setup(x => x.GetProfileAsync(targetId, _userId)).ThrowsAsync(new KeyNotFoundException());

        var result = await _controller.GetUserProfile(targetId);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task GetUserProfile_Exception_Returns500()
    {
        var targetId = Guid.NewGuid().ToString();
        _userServiceMock.Setup(x => x.GetProfileAsync(targetId, _userId)).ThrowsAsync(new Exception());

        var result = await _controller.GetUserProfile(targetId);

        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusCodeResult.StatusCode);
    }

    [Fact]
    public async Task UpdateProfile_InvalidModel_ReturnsBadRequest()
    {
        _controller.ModelState.AddModelError("Bio", "Too long");
        var result = await _controller.UpdateProfile(new UpdateProfileRequest());

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task UpdateProfile_ReturnsOk()
    {
        var req = new UpdateProfileRequest();
        var profile = new UserProfileDto();
        _userServiceMock.Setup(x => x.UpdateProfileAsync(_userId, req)).ReturnsAsync(profile);

        var result = await _controller.UpdateProfile(req);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(profile, okResult.Value);
    }

    [Fact]
    public async Task UpdateProfile_Exception_Returns500()
    {
        var req = new UpdateProfileRequest();
        _userServiceMock.Setup(x => x.UpdateProfileAsync(_userId, req)).ThrowsAsync(new Exception());

        var result = await _controller.UpdateProfile(req);

        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusCodeResult.StatusCode);
    }

    [Fact]
    public async Task ToggleFollow_ReturnsOk()
    {
        var targetId = Guid.NewGuid().ToString();
        _userServiceMock.Setup(x => x.ToggleFollowAsync(_userId, targetId)).ReturnsAsync(true);

        var result = await _controller.ToggleFollow(targetId);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task ToggleFollow_InvalidOperation_ReturnsBadRequest()
    {
        var targetId = Guid.NewGuid().ToString();
        _userServiceMock.Setup(x => x.ToggleFollowAsync(_userId, targetId)).ThrowsAsync(new InvalidOperationException());

        var result = await _controller.ToggleFollow(targetId);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task ToggleFollow_Exception_Returns500()
    {
        var targetId = Guid.NewGuid().ToString();
        _userServiceMock.Setup(x => x.ToggleFollowAsync(_userId, targetId)).ThrowsAsync(new Exception());

        var result = await _controller.ToggleFollow(targetId);

        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusCodeResult.StatusCode);
    }

    [Fact]
    public async Task SearchUsers_EmptyQuery_ReturnsBadRequest()
    {
        var result = await _controller.SearchUsers("  ");
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task SearchUsers_ReturnsOk()
    {
        var query = "test";
        var users = new List<UserSearchDto>();
        _userServiceMock.Setup(x => x.SearchUsersAsync(query, _userId)).ReturnsAsync(users);

        var result = await _controller.SearchUsers(query);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(users, okResult.Value);
    }

    [Fact]
    public async Task SearchUsers_Exception_Returns500()
    {
        var query = "test";
        _userServiceMock.Setup(x => x.SearchUsersAsync(query, _userId)).ThrowsAsync(new Exception());

        var result = await _controller.SearchUsers(query);

        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusCodeResult.StatusCode);
    }
}