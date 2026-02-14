namespace FractPal.Service.Implementation;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FractPal.Model.Entities;
using FractPal.Service.Interface;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

public class JwtService : IJwtService
{
    private readonly IConfiguration _configuration;

    public JwtService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(User user)
    {
        // Try environment variables first, then fall back to appsettings
        var secretKey = _configuration["JWT_SECRET_KEY"]
            ?? _configuration["JwtSettings:SecretKey"]
            ?? throw new InvalidOperationException("JWT SecretKey not configured");

        var issuer = _configuration["JWT_ISSUER"]
            ?? _configuration["JwtSettings:Issuer"]
            ?? "FractPal";

        var audience = _configuration["JWT_AUDIENCE"]
            ?? _configuration["JwtSettings:Audience"]
            ?? "FractPal";

        var expiryMinutes = int.Parse(
            _configuration["JWT_EXPIRY_MINUTES"]
            ?? _configuration["JwtSettings:ExpiryMinutes"]
            ?? "1440"
        );

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
            new Claim(ClaimTypes.Name, user.UserName ?? ""),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
