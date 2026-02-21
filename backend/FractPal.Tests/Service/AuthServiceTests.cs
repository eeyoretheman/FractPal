using FractPal.Model.DTO.Auth;
using FractPal.Model.Entities;
using FractPal.Service.Implementation;
using FractPal.Service.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Moq;
using Xunit;
using System.Collections.Generic;

namespace FractPal.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<UserManager<FractPalUser>> _userManagerMock;
    private readonly Mock<SignInManager<FractPalUser>> _signInManagerMock;
    private readonly Mock<IJwtService> _jwtServiceMock;
    private readonly AuthService _service;

    public AuthServiceTests()
    {
        var store = new Mock<IUserStore<FractPalUser>>();
        _userManagerMock = new Mock<UserManager<FractPalUser>>(store.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        var contextAccessor = new Mock<IHttpContextAccessor>();
        var claimsFactory = new Mock<IUserClaimsPrincipalFactory<FractPalUser>>();
        _signInManagerMock = new Mock<SignInManager<FractPalUser>>(_userManagerMock.Object, contextAccessor.Object, claimsFactory.Object, null!, null!, null!, null!);

        _jwtServiceMock = new Mock<IJwtService>();

        // Ensure GetRolesAsync always returns an empty list to avoid null-reference in service logic
        _userManagerMock.Setup(x => x.GetRolesAsync(It.IsAny<FractPalUser>())).ReturnsAsync(new List<string>());

        _service = new AuthService(_userManagerMock.Object, _signInManagerMock.Object, _jwtServiceMock.Object);
    }

    [Fact]
    public async Task Login_InvalidEmail_ThrowsUnauthorizedAccessException()
    {
        var request = new LoginRequest { Email = "test@test.com", Password = "password" };
        _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email)).ReturnsAsync((FractPalUser?)null);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.Login(new DefaultHttpContext(), request));
    }

    [Fact]
    public async Task Login_InvalidPassword_ThrowsUnauthorizedAccessException()
    {
        var request = new LoginRequest { Email = "test@test.com", Password = "password" };
        var user = new FractPalUser();
        _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email)).ReturnsAsync(user);
        _signInManagerMock.Setup(x => x.CheckPasswordSignInAsync(user, request.Password, false)).ReturnsAsync(SignInResult.Failed);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.Login(new DefaultHttpContext(), request));
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsResponse()
    {
        var request = new LoginRequest { Email = "test@test.com", Password = "password" };
        var user = new FractPalUser { Id = Guid.NewGuid(), UserName = "user", Email = "test@test.com" };
        _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email)).ReturnsAsync(user);
        _signInManagerMock.Setup(x => x.CheckPasswordSignInAsync(user, request.Password, false)).ReturnsAsync(SignInResult.Success);
        _jwtServiceMock.Setup(x => x.GenerateToken(user)).ReturnsAsync("token");

        var response = await _service.Login(new DefaultHttpContext(), request);

        Assert.NotNull(response);
        Assert.Equal("token", response.Token);
        Assert.Equal(user.Id.ToString(), response.Id);
        Assert.Equal(user.UserName, response.Username);
        Assert.Equal(user.Email, response.Email);
    }

    [Fact]
    public async Task Register_ExistingEmail_ThrowsInvalidOperationException()
    {
        var request = new RegistrationRequest { Email = "test@test.com", Username = "user", Password = "password" };
        _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email)).ReturnsAsync(new FractPalUser());

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.Register(request));
    }

    [Fact]
    public async Task Register_ExistingUsername_ThrowsInvalidOperationException()
    {
        var request = new RegistrationRequest { Email = "test@test.com", Username = "user", Password = "password" };
        _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email)).ReturnsAsync((FractPalUser?)null);
        _userManagerMock.Setup(x => x.FindByNameAsync(request.Username)).ReturnsAsync(new FractPalUser());

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.Register(request));
    }

    [Fact]
    public async Task Register_CreateFails_ThrowsInvalidOperationException()
    {
        var request = new RegistrationRequest { Email = "test@test.com", Username = "user", Password = "password" };
        _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email)).ReturnsAsync((FractPalUser?)null);
        _userManagerMock.Setup(x => x.FindByNameAsync(request.Username)).ReturnsAsync((FractPalUser?)null);
        _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<FractPalUser>(), request.Password)).ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Error" }));

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.Register(request));
    }

    [Fact]
    public async Task Register_Success_ReturnsResponse()
    {
        var request = new RegistrationRequest { Email = "test@test.com", Username = "user", Password = "password" };
        _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email)).ReturnsAsync((FractPalUser?)null);
        _userManagerMock.Setup(x => x.FindByNameAsync(request.Username)).ReturnsAsync((FractPalUser?)null);
        _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<FractPalUser>(), request.Password)).ReturnsAsync(IdentityResult.Success);

        var response = await _service.Register(request);

        Assert.NotNull(response);
        Assert.Equal(request.Username, response.Username);
        Assert.Equal(request.Email, response.Email);
    }
}
