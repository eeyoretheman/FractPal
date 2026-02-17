namespace FractPal.Service.Implementation;

using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FractPal.Model.Entities;
using FractPal.Service.Interface;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

public class JwtService(IConfiguration configuration) : IJwtService
{
    private readonly IConfiguration configuration = configuration;

    public string GenerateToken(User user)
    {
        // Try environment variables first, then fall back to appsettings
        var secretKey = this.configuration["JWT_SECRET_KEY"]
            ?? this.configuration["JwtSettings:SecretKey"]
            ?? throw new InvalidOperationException("JWT SecretKey not configured");

        var issuer = this.configuration["JWT_ISSUER"]
            ?? this.configuration["JwtSettings:Issuer"]
            ?? "FractPal";

        var audience = this.configuration["JWT_AUDIENCE"]
            ?? this.configuration["JwtSettings:Audience"]
            ?? "FractPal";

        var expiryMinutes = int.Parse(
            this.configuration["JWT_EXPIRY_MINUTES"]
            ?? this.configuration["JwtSettings:ExpiryMinutes"]
            ?? "60",
            new CultureInfo("en-US")
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
