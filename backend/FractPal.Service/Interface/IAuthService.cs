namespace FractPal.Service.Interface;

using FractPal.Model.DTO.Auth;
using Microsoft.AspNetCore.Http;

public interface IAuthService
{
    public Task<LoginResponse> Login(HttpContext context, LoginRequest request);
    public Task<RegistrationResponse> Register(RegistrationRequest request);
}
