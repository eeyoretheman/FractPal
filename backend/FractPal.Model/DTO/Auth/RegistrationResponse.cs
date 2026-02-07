using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FractPal.Model.DTO.Auth;

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
