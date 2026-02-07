using FractPal.Service.Interface;
using FractPal.Model.DTO.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FractPal.API.Controllers
{
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
            if(dto == null || !ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                LoginResponse response = await _authService.Login(HttpContext, dto);
                return Ok(response);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { message = "Invalid email or password" });
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegistrationRequest dto)
        {
            if(dto == null || !ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                RegistrationResponse response = await _authService.Register(dto);
                return CreatedAtAction(nameof(Register), new { id = response.Id }, response);
            }
            catch (InvalidOperationException ex)
            {
                // User already exists or registration failed
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
