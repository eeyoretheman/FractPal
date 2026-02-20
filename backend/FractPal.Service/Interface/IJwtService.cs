namespace FractPal.Service.Interface;

using FractPal.Model.Entities;

/// <summary>
/// Provides JWT token generation for authenticated users.
/// </summary>
public interface IJwtService
{
    /// <summary>
    /// Generates a signed JWT access token for the given user.
    /// </summary>
    /// <param name="user">The user for whom the token will be issued.</param>
    /// <returns>A signed JWT string.</returns>
    public string GenerateToken(FractPalUser user);
}
