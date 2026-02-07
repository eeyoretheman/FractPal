namespace FractPal.Model.DTO.Auth;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// DTO for user registration
/// </summary>
public class RegistrationRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Required]
    public string? Password { get; set; }
}
