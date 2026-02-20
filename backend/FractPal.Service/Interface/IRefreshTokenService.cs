namespace FractPal.Service.Interface;

using Microsoft.AspNetCore.Identity;

/// <summary>
/// Manages refresh token lifecycle: generation, validation, and invalidation.
/// </summary>
public interface IRefreshTokenService
{
    /// <summary>
    /// Generates a new cryptographically secure refresh token for the specified user
    /// and persists it for future validation.
    /// </summary>
    /// <param name="user">The user to generate a refresh token for.</param>
    /// <returns>The generated refresh token string.</returns>
    public Task<string> GenerateRefreshToken(IdentityUser user);

    /// <summary>
    /// Validates whether the given refresh token is active and belongs to the specified user.
    /// </summary>
    /// <param name="userId">The ID of the user claiming the token.</param>
    /// <param name="refreshToken">The refresh token to validate.</param>
    /// <returns><c>true</c> if the token is valid; otherwise <c>false</c>.</returns>
    public Task<bool> ValidateRefreshToken(Guid userId, string refreshToken);

    /// <summary>
    /// Revokes a refresh token so it can no longer be used to obtain new access tokens.
    /// </summary>
    /// <param name="refreshToken">The refresh token to invalidate.</param>
    public Task InvalidateRefreshToken(string refreshToken);

    /// <summary>
    /// Looks up the user ID associated with the given refresh token.
    /// </summary>
    /// <param name="refreshToken">The refresh token to look up.</param>
    /// <returns>The associated user's <see cref="Guid"/>, or <c>null</c> if the token is not found or expired.</returns>
    public Task<Guid?> GetUserIdByRefreshToken(string refreshToken);
}
