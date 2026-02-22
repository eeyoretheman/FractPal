using FractPal.Model.Entity;
using FractPal.Service.Implementation;
using FractPal.Service.Interface;
using Microsoft.AspNetCore.Identity;
using MockQueryable;
using MockQueryable.Moq;
using Moq;
using Xunit;

namespace FractPal.Tests.Services;

public class RefreshTokenServiceTests
{
    private readonly Mock<IRepository<RefreshToken>> _repositoryMock;
    private readonly RefreshTokenService _service;

    public RefreshTokenServiceTests()
    {
        _repositoryMock = new Mock<IRepository<RefreshToken>>();
        _service = new RefreshTokenService(_repositoryMock.Object);
    }

    [Fact]
    public async Task GenerateRefreshToken_Success_ReturnsToken()
    {
        var user = new IdentityUser { Id = Guid.NewGuid().ToString() };
        _repositoryMock.Setup(x => x.AddAsync(It.IsAny<RefreshToken>())).Returns(Task.CompletedTask);
        _repositoryMock.Setup(x => x.CommitAsync()).ReturnsAsync(1);

        var token = await _service.GenerateRefreshToken(user);

        Assert.NotNull(token);
        _repositoryMock.Verify(x => x.AddAsync(It.IsAny<RefreshToken>()), Times.Once);
        _repositoryMock.Verify(x => x.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task GetUserIdByRefreshToken_TokenNotFound_ReturnsNull()
    {
        var tokens = new List<RefreshToken>();
        _repositoryMock.Setup(x => x.Query()).Returns(tokens.BuildMock());

        var result = await _service.GetUserIdByRefreshToken("token");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetUserIdByRefreshToken_TokenRevoked_ReturnsNull()
    {
        var tokens = new List<RefreshToken>
        {
            new RefreshToken { Token = "token", UserId = Guid.NewGuid(), IsRevoked = true }
        };
        _repositoryMock.Setup(x => x.Query()).Returns(tokens.BuildMock());

        var result = await _service.GetUserIdByRefreshToken("token");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetUserIdByRefreshToken_Success_ReturnsUserId()
    {
        var userId = Guid.NewGuid();
        var tokens = new List<RefreshToken>
        {
            new RefreshToken { Token = "token", UserId = userId, IsRevoked = false }
        };
        _repositoryMock.Setup(x => x.Query()).Returns(tokens.BuildMock());

        var result = await _service.GetUserIdByRefreshToken("token");

        Assert.Equal(userId, result);
    }

    [Fact]
    public async Task InvalidateRefreshToken_TokenNotFound_DoesNothing()
    {
        var tokens = new List<RefreshToken>();
        _repositoryMock.Setup(x => x.Query()).Returns(tokens.BuildMock());

        await _service.InvalidateRefreshToken("token");

        _repositoryMock.Verify(x => x.CommitAsync(), Times.Never);
    }

    [Fact]
    public async Task InvalidateRefreshToken_Success_RevokesToken()
    {
        var token = new RefreshToken { Token = "token", IsRevoked = false };
        var tokens = new List<RefreshToken> { token };
        _repositoryMock.Setup(x => x.Query()).Returns(tokens.BuildMock());

        await _service.InvalidateRefreshToken("token");

        Assert.True(token.IsRevoked);
        _repositoryMock.Verify(x => x.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task ValidateRefreshToken_TokenNotFound_ReturnsFalse()
    {
        var tokens = new List<RefreshToken>();
        _repositoryMock.Setup(x => x.Query()).Returns(tokens.BuildMock());

        var result = await _service.ValidateRefreshToken(Guid.NewGuid(), "token");

        Assert.False(result);
    }

    [Fact]
    public async Task ValidateRefreshToken_TokenRevoked_ReturnsFalse()
    {
        var userId = Guid.NewGuid();
        var tokens = new List<RefreshToken>
        {
            new RefreshToken { Token = "token", UserId = userId, IsRevoked = true }
        };
        _repositoryMock.Setup(x => x.Query()).Returns(tokens.BuildMock());

        var result = await _service.ValidateRefreshToken(userId, "token");

        Assert.False(result);
    }

    [Fact]
    public async Task ValidateRefreshToken_IncorrectUserId_ReturnsFalse()
    {
        var userId = Guid.NewGuid();
        var tokens = new List<RefreshToken>
        {
            new RefreshToken { Token = "token", UserId = Guid.NewGuid(), IsRevoked = false }
        };
        _repositoryMock.Setup(x => x.Query()).Returns(tokens.BuildMock());

        var result = await _service.ValidateRefreshToken(userId, "token");

        Assert.False(result);
    }

    [Fact]
    public async Task ValidateRefreshToken_Success_ReturnsTrue()
    {
        var userId = Guid.NewGuid();
        var tokens = new List<RefreshToken>
        {
            new RefreshToken { Token = "token", UserId = userId, IsRevoked = false }
        };
        _repositoryMock.Setup(x => x.Query()).Returns(tokens.BuildMock());

        var result = await _service.ValidateRefreshToken(userId, "token");

        Assert.True(result);
    }
}
