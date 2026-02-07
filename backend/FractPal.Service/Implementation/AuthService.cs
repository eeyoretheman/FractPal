namespace FractPal.Service.Implementation;

using FractPal.Service.Interface;
using FractPal.Model.DTO.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System;
using System.Linq;
using System.Threading.Tasks;


public class AuthService : IAuthService
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IJwtService _jwtService;
    private readonly IRefreshTokenService _refreshTokenService;

    public AuthService(UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IJwtService jwtService, IRefreshTokenService refreshTokenService)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _jwtService = jwtService;
        _refreshTokenService = refreshTokenService;
    }
    public async Task<LoginResponse> Login(HttpContext context, LoginRequest dto)
    {
        IdentityUser? user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null || !(await _userManager.CheckPasswordAsync(user, dto.Password)))
        {
            throw new UnauthorizedAccessException("Invalid credentials");
        }

        string token = await _jwtService.GenerateJwt(user);
        string refresh = await _refreshTokenService.GenerateRefreshToken(user);

        return new LoginResponse()
        {
            Jwt = token,
            RefreshToken = refresh
        };
    }

    public async Task<RegistrationResponse> Register(RegistrationRequest dto)
    {
        IdentityUser existing = await _userManager.FindByEmailAsync(dto.Email);
        if(existing != null)
        {
            throw new InvalidOperationException("Already registered");
        }

        IdentityUser user = new IdentityUser
        {
            Email = dto.Email,
            UserName = dto.Email
        };
        string password = dto.Password;

        IdentityResult result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException("Something went wrong...");
        }

        await _userManager.AddToRoleAsync(user, "User");

        var roles = await _userManager.GetRolesAsync(user);
        RegistrationResponse userResponse = new RegistrationResponse() {
            Id = Guid.Parse(user.Id),
            Email = user.Email,
            Roles = roles.ToList()
        };
        return userResponse;
    }
}
