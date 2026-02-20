namespace FractPal.Service.Interface;

using FractPal.Model.DTO.Auth;
using Microsoft.AspNetCore.Http;

/// <summary>
/// Provides authentication operations for user login and registration.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Authenticates a user with the provided credentials and issues an access token.
    /// Sets any necessary cookies or session state via <paramref name="context"/>.
    /// </summary>
    /// <param name="context">The current HTTP context, used to set authentication cookies if applicable.</param>
    /// <param name="request">The login credentials containing email and password.</param>
    /// <returns>A <see cref="LoginResponse"/> containing the access token and user information.</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown when the email/password combination is invalid.</exception>
    public Task<LoginResponse> Login(HttpContext context, LoginRequest request);

    /// <summary>
    /// Registers a new user account with the provided details.
    /// </summary>
    /// <param name="request">The registration data including username, email and password.</param>
    /// <returns>A <see cref="RegistrationResponse"/> confirming the created account.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the email or username is already in use.</exception>
    public Task<RegistrationResponse> Register(RegistrationRequest request);
}
