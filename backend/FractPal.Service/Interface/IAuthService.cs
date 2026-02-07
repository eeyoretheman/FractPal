using FractPal.Model.DTO.Auth;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FractPal.Service.Interface;

/// <summary>
/// Defines methods for user authentication
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Authenticates a user using email and password
    /// </summary>
    /// <returns>JWT and refresh tokens</returns>
    Task<LoginResponseDTO> Login(HttpContext context, LoginRequest dto);

    /// <summary>
    /// Registers a new user
    /// </summary>
    /// <param name="dto">User data</param>
    /// <returns>The registered user object</returns>
    Task<RegistrationResponseDTO> Register(RegistrationRequest dto);
}
