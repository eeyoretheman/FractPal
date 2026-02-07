namespace FractPal.Model.DTO.Auth;

using System;
using System.Collections.Generic;

/// <summary>
/// Represents user profile information
/// </summary>
public class RegistrationResponse
{
    /// <summary>
    /// Gets or sets the unique identifier of the user
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the user email
    /// </summary>
    public string Email { get; set; } = default!;

    /// <summary>
    /// Gets or sets the list of roles assigned to the user
    /// </summary>
    public List<string> Roles { get; set; } = [];
}
