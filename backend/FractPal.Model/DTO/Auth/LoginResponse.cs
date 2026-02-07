using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FractPal.Model.DTO.Auth;

/// <summary>
/// DTO representing the response returned after success
/// </summary>
public class LoginResponse
{
    /// <summary>
    /// Gets or sets the issued token
    /// </summary>
    public string JWT { get; set; } = default!;

    /// <summary>
    /// Gets or sets the refresh token
    /// </summary>
    public string RefreshToken { get; set; } = default!;
}
