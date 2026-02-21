namespace FractPal.Model.DTO.Auth;

using System.ComponentModel.DataAnnotations;

public class LoginRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

public class LoginResponse
{
    public string Id { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;

    /// <summary>Indicates whether the authenticated user has the Admin role.</summary>
    public bool IsAdmin { get; set; }
}

public class RegistrationRequest
{
    [Required]
    public string Username { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;
}

public class RegistrationResponse
{
    public string Id { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    /// <summary>Indicates whether the newly registered user has the Admin role.</summary>
    public bool IsAdmin { get; set; }
}
