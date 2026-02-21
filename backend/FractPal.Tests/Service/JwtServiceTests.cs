using System.IdentityModel.Tokens.Jwt;
using FractPal.Model.Entities;
using FractPal.Service.Implementation;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace FractPal.Tests.Services;

public class JwtServiceTests
{
    [Fact]
    public void GenerateToken_MissingSecretKey_ThrowsInvalidOperationException()
    {
        var configMock = new Mock<IConfiguration>();
        configMock.Setup(x => x["JWT_SECRET_KEY"]).Returns((string)null!);
        configMock.Setup(x => x["JwtSettings:SecretKey"]).Returns((string)null!);

        var service = new JwtService(configMock.Object);
        var user = new FractPalUser { Id = Guid.NewGuid(), Email = "test@test.com", UserName = "test" };

        Assert.Throws<InvalidOperationException>(() => service.GenerateToken(user));
    }

    [Fact]
    public void GenerateToken_Success_ReturnsToken()
    {
        var configMock = new Mock<IConfiguration>();
        configMock.Setup(x => x["JWT_SECRET_KEY"]).Returns("this_is_a_very_long_secret_key_for_testing");
        configMock.Setup(x => x["JWT_ISSUER"]).Returns("issuer");
        configMock.Setup(x => x["JWT_AUDIENCE"]).Returns("audience");
        configMock.Setup(x => x["JWT_EXPIRY_MINUTES"]).Returns("60");

        var service = new JwtService(configMock.Object);
        var user = new FractPalUser { Id = Guid.NewGuid(), Email = "test@test.com", UserName = "test" };

        var token = service.GenerateToken(user);

        Assert.NotNull(token);

        var handler = new JwtSecurityTokenHandler();
        Assert.True(handler.CanReadToken(token));

        var jwtToken = handler.ReadJwtToken(token);
        Assert.Equal("issuer", jwtToken.Issuer);
        Assert.Equal("audience", jwtToken.Audiences.First());
    }
}