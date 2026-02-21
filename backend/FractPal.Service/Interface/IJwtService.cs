namespace FractPal.Service.Interface;

using FractPal.Model.Entities;

/// <summary>
/// Provides JWT token generation for authenticated users.
/// </summary>
public interface IJwtService
{
    /// <summary>
    /// Generates a signed JWT access token for the given user, including their role claims.
    /// </summary>
    /// <param name="user">The authenticated <see cref="FractPalUser"/> for whom the token is issued.</param>
    /// <returns>A signed JWT string.</returns>
    public Task<string> GenerateToken(FractPalUser user);
}
