namespace FractPal.Service.Implementation;

using FractPal.Model.Entity;
using FractPal.Service.Interface;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

/// <summary>
/// Manages refresh token lifecycle: generation, validation, lookup, and revocation.
/// Tokens are cryptographically random, stored in the repository, and expire after 7 days.
/// </summary>
public class RefreshTokenService(IRepository<RefreshToken> refreshTokenRepository) : IRefreshTokenService
{
    private readonly IRepository<RefreshToken> refreshTokenRepository = refreshTokenRepository;

    /// <inheritdoc/>
    public async Task<string> GenerateRefreshToken(IdentityUser user)
    {
        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var refreshToken = new RefreshToken()
        {
            Token = token,
            UserId = Guid.Parse(user.Id),
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsRevoked = false
        };

        await this.refreshTokenRepository.AddAsync(refreshToken);
        await this.refreshTokenRepository.CommitAsync();
        return token;
    }

    /// <inheritdoc/>
    public async Task<Guid?> GetUserIdByRefreshToken(string refreshToken)
    {
        var token = await this.refreshTokenRepository.Query()
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken && !rt.IsRevoked);
        if (token != null)
        {
            return token.UserId;
        }

        return null;
    }

    /// <inheritdoc/>
    public async Task InvalidateRefreshToken(string refreshToken)
    {
        var token = await this.refreshTokenRepository.Query()
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken);
        if (token != null)
        {
            token.IsRevoked = true;
            await this.refreshTokenRepository.CommitAsync();
        }
    }

    /// <inheritdoc/>
    public async Task<bool> ValidateRefreshToken(Guid userId, string refreshToken)
    {
        var token = await this.refreshTokenRepository.Query()
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken && rt.IsRevoked == false && rt.UserId == userId);
        return token != null;
    }
}
