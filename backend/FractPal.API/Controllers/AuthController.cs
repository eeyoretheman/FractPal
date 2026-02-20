namespace FractPal.API.Controllers;

using FractPal.Service.Interface;
using FractPal.Model.DTO.Auth;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Handles authentication operations including user login and registration.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class AuthController(IAuthService authService) : ControllerBase
{
    private readonly IAuthService authService = authService;

    /// <summary>
    /// Authenticates a user and returns an access token.
    /// </summary>
    /// <param name="dto">The login credentials containing email and password.</param>
    /// <returns>
    /// <see cref="OkObjectResult"/> with a <see cref="LoginResponse"/> on success;
    /// <see cref="BadRequestObjectResult"/> if the request body is invalid;
    /// <see cref="UnauthorizedObjectResult"/> if credentials are incorrect.
    /// </returns>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest dto)
    {
        if (dto == null || !this.ModelState.IsValid)
        {
            return this.BadRequest(this.ModelState);
        }

        try
        {
            var response = await this.authService.Login(this.HttpContext, dto);
            return this.Ok(response);
        }
        catch (UnauthorizedAccessException)
        {
            return this.Unauthorized(new { message = "Invalid email or password" });
        }
    }

    /// <summary>
    /// Registers a new user account.
    /// </summary>
    /// <param name="dto">The registration details including username, email and password.</param>
    /// <returns>
    /// <see cref="OkObjectResult"/> with a <see cref="RegistrationResponse"/> on success;
    /// <see cref="BadRequestObjectResult"/> if the request body is invalid or the username/email is already taken.
    /// </returns>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegistrationRequest dto)
    {
        if (dto == null || !this.ModelState.IsValid)
        {
            return this.BadRequest(this.ModelState);
        }

        try
        {
            var response = await this.authService.Register(dto);
            return this.Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return this.BadRequest(new { message = ex.Message });
        }
    }
}
