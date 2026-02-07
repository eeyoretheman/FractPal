namespace FractPal.Service.Interface;

using Microsoft.AspNetCore.Identity;
using System;
using System.Threading.Tasks;

public interface IRefreshTokenService
{
    Task<string> GenerateRefreshToken(IdentityUser user);

    Task<bool> ValidateRefreshToken(Guid userId, string refreshToken);

    Task InvalidateRefreshToken(string refreshToken);

    Task<Guid?> GetUserIdByRefreshToken(string refreshToken);
}
