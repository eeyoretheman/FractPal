namespace FractPal.API.Controllers;

using FractPal.Service.Interface;
using FractPal.Model.DTO.Auth;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
public class AuthController(IAuthService authService) : ControllerBase
{
    private readonly IAuthService authService = authService;

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
