namespace FractPal.Service.Implementation;

using FractPal.Model.DTO.Auth;
using FractPal.Model.Entities;
using FractPal.Service.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

public class AuthService : IAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly IJwtService _jwtService;

    public AuthService(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        IJwtService jwtService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtService = jwtService;
    }

    public async Task<LoginResponse> Login(HttpContext context, LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user == null)
        {
            throw new UnauthorizedAccessException("Invalid credentials");
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);

        if (!result.Succeeded)
        {
            throw new UnauthorizedAccessException("Invalid credentials");
        }

        var token = _jwtService.GenerateToken(user);

        return new LoginResponse
        {
            Id = user.Id,
            Username = user.UserName ?? "",
            Email = user.Email ?? "",
            Token = token
        };
    }

    public async Task<RegistrationResponse> Register(RegistrationRequest request)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);

        if (existingUser != null)
        {
            throw new InvalidOperationException("User with this email already exists");
        }

        var existingUsername = await _userManager.FindByNameAsync(request.Username);

        if (existingUsername != null)
        {
            throw new InvalidOperationException("Username already taken");
        }

        var user = new User
        {
            UserName = request.Username,
            Email = request.Email,
            JoinedDate = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Registration failed: {errors}");
        }

        return new RegistrationResponse
        {
            Id = user.Id,
            Username = user.UserName,
            Email = user.Email
        };
    }
}
