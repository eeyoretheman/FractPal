namespace FractPal.Model.DTO.Auth;

/// <summary>
/// DTO representing the response returned after success
/// </summary>
public class LoginResponse
{
    /// <summary>
    /// Gets or sets the issued token
    /// </summary>
    public string Jwt { get; set; } = default!;

    /// <summary>
    /// Gets or sets the refresh token
    /// </summary>
    public string RefreshToken { get; set; } = default!;
}
