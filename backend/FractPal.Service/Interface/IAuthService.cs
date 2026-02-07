namespace FractPal.Service.Interface;

using FractPal.Model.DTO.Auth;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

/// <summary>
/// Defines methods for user authentication
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Authenticates a user using email and password
    /// </summary>
    /// <returns>JWT and refresh tokens</returns>
    Task<LoginResponse> Login(HttpContext context, LoginRequest dto);

    /// <summary>
    /// Registers a new user
    /// </summary>
    /// <param name="dto">User data</param>
    /// <returns>The registered user object</returns>
    Task<RegistrationResponse> Register(RegistrationRequest dto);
}
