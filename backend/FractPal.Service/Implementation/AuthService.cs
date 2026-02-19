namespace FractPal.Service.Implementation;

using FractPal.Model.DTO.Auth;
using FractPal.Model.Entities;
using FractPal.Service.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

public class AuthService(
    UserManager<FractPalUser> userManager,
    SignInManager<FractPalUser> signInManager,
    IJwtService jwtService) : IAuthService
{
    private readonly UserManager<FractPalUser> userManager = userManager;
    private readonly SignInManager<FractPalUser> signInManager = signInManager;
    private readonly IJwtService jwtService = jwtService;

    public async Task<LoginResponse> Login(HttpContext context, LoginRequest request)
    {
        var user = await this.userManager.FindByEmailAsync(request.Email);

        if (user == null)
        {
            throw new UnauthorizedAccessException("Invalid credentials");
        }

        var result = await this.signInManager.CheckPasswordSignInAsync(user, request.Password, false);

        if (!result.Succeeded)
        {
            throw new UnauthorizedAccessException("Invalid credentials");
        }

        var token = this.jwtService.GenerateToken(user);

        return new LoginResponse
        {
            Id = user.Id.ToString(),
            Username = user.UserName ?? "",
            Email = user.Email ?? "",
            Token = token
        };
    }

    public async Task<RegistrationResponse> Register(RegistrationRequest request)
    {
        var existingUser = await this.userManager.FindByEmailAsync(request.Email);

        if (existingUser != null)
        {
            throw new InvalidOperationException("User with this email already exists");
        }

        var existingUsername = await this.userManager.FindByNameAsync(request.Username);

        if (existingUsername != null)
        {
            throw new InvalidOperationException("Username already taken");
        }

        var user = new FractPalUser
        {
            UserName = request.Username,
            Email = request.Email,
            JoinedDate = DateTime.UtcNow
        };

        var result = await this.userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Registration failed: {errors}");
        }

        return new RegistrationResponse
        {
            Id = user.Id.ToString(),
            Username = user.UserName,
            Email = user.Email
        };
    }
}
