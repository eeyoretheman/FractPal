namespace FractPal.API.Controllers;

using FractPal.Service.Interface;
using FractPal.Model.DTO.Auth;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest dto)
    {
        if (dto == null || !ModelState.IsValid)
        {
            return this.BadRequest(ModelState);
        }

        try
        {
            LoginResponse response = await this._authService.Login(HttpContext, dto);
            return this.Ok(response);
        }
        catch (UnauthorizedAccessException)
        {
            return this.Unauthorized(new { message = "Invalid email or password" });
        }
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegistrationRequest dto)
    {
        if (dto == null || !ModelState.IsValid)
        {
            return this.BadRequest(ModelState);
        }

        try
        {
            RegistrationResponse response = await _authService.Register(dto);
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return this.BadRequest(new { message = ex.Message });
        }
    }
}
