using System.IdentityModel.Tokens.Jwt;
using System.Collections.Generic;
using FractPal.Model.Entities;
using FractPal.Service.Implementation;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Identity;
using Moq;
using Xunit;

namespace FractPal.Tests.Services;

public class JwtServiceTests
{
    [Fact]
    public async Task GenerateToken_MissingSecretKey_ThrowsInvalidOperationException()
    {
        var configMock = new Mock<IConfiguration>();
        configMock.Setup(x => x["JWT_SECRET_KEY"]).Returns((string?)null);
        configMock.Setup(x => x["JwtSettings:SecretKey"]).Returns((string?)null);

        var store = new Mock<IUserStore<FractPalUser>>();
        var userManagerMock = new Mock<UserManager<FractPalUser>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        var service = new JwtService(configMock.Object, userManagerMock.Object);
        var user = new FractPalUser { Id = Guid.NewGuid(), Email = "test@test.com", UserName = "test" };

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.GenerateToken(user));
    }

    [Fact]
    public async Task GenerateToken_Success_ReturnsToken()
    {
        var configMock = new Mock<IConfiguration>();
        configMock.Setup(x => x["JWT_SECRET_KEY"]).Returns("this_is_a_very_long_secret_key_for_testing");
        configMock.Setup(x => x["JWT_ISSUER"]).Returns("issuer");
        configMock.Setup(x => x["JWT_AUDIENCE"]).Returns("audience");
        configMock.Setup(x => x["JWT_EXPIRY_MINUTES"]).Returns("60");

        var store = new Mock<IUserStore<FractPalUser>>();
        var userManagerMock = new Mock<UserManager<FractPalUser>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        var service = new JwtService(configMock.Object, userManagerMock.Object);
        var user = new FractPalUser { Id = Guid.NewGuid(), Email = "test@test.com", UserName = "test" };

        userManagerMock.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(new List<string>());

        var token = await service.GenerateToken(user);

        Assert.NotNull(token);

        var handler = new JwtSecurityTokenHandler();
        Assert.True(handler.CanReadToken(token));

        var jwtToken = handler.ReadJwtToken(token);
        Assert.Equal("issuer", jwtToken.Issuer);
        Assert.Equal("audience", jwtToken.Audiences.First());
    }
}
