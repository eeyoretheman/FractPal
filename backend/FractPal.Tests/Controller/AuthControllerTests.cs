using FractPal.API.Controllers;
using FractPal.Model.DTO.Auth;
using FractPal.Service.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace FractPal.Tests.Controllers;

public class AuthControllerTests
{
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _authServiceMock = new Mock<IAuthService>();
        _controller = new AuthController(_authServiceMock.Object);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
    }

    [Fact]
    public async Task Login_NullDto_ReturnsBadRequest()
    {
        var result = await _controller.Login(null!);
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Login_InvalidModel_ReturnsBadRequest()
    {
        _controller.ModelState.AddModelError("Email", "Required");
        var dto = new LoginRequest();
        var result = await _controller.Login(dto);
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Login_ValidRequest_ReturnsOk()
    {
        var dto = new LoginRequest { Email = "test@test.com", Password = "password" };
        var response = new LoginResponse { Token = "token", Id = "1", Username = "user", Email = "test@test.com" };
        _authServiceMock.Setup(x => x.Login(It.IsAny<HttpContext>(), dto)).ReturnsAsync(response);

        var result = await _controller.Login(dto);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(response, okResult.Value);
    }

    [Fact]
    public async Task Login_InvalidCredentials_ReturnsUnauthorized()
    {
        var dto = new LoginRequest { Email = "test@test.com", Password = "wrong" };
        _authServiceMock.Setup(x => x.Login(It.IsAny<HttpContext>(), dto)).ThrowsAsync(new UnauthorizedAccessException());

        var result = await _controller.Login(dto);

        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.NotNull(unauthorizedResult.Value);
    }

    [Fact]
    public async Task Register_NullDto_ReturnsBadRequest()
    {
        var result = await _controller.Register(null!);
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Register_InvalidModel_ReturnsBadRequest()
    {
        _controller.ModelState.AddModelError("Email", "Required");
        var dto = new RegistrationRequest();
        var result = await _controller.Register(dto);
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Register_ValidRequest_ReturnsOk()
    {
        var dto = new RegistrationRequest { Email = "test@test.com", Password = "password", Username = "user" };
        var response = new RegistrationResponse { Id = "1", Username = "user", Email = "test@test.com" };
        _authServiceMock.Setup(x => x.Register(dto)).ReturnsAsync(response);

        var result = await _controller.Register(dto);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(response, okResult.Value);
    }

    [Fact]
    public async Task Register_UserExists_ReturnsBadRequest()
    {
        var dto = new RegistrationRequest { Email = "test@test.com", Password = "password", Username = "user" };
        _authServiceMock.Setup(x => x.Register(dto)).ThrowsAsync(new InvalidOperationException("User exists"));

        var result = await _controller.Register(dto);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequestResult.Value);
    }
}
