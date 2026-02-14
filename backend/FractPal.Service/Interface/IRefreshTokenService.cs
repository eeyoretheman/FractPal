namespace FractPal.Service.Interface;

using Microsoft.AspNetCore.Identity;
using System;
using System.Threading.Tasks;

public interface IRefreshTokenService
{
    public Task<string> GenerateRefreshToken(IdentityUser user);

    public Task<bool> ValidateRefreshToken(Guid userId, string refreshToken);

    public Task InvalidateRefreshToken(string refreshToken);

    public Task<Guid?> GetUserIdByRefreshToken(string refreshToken);
}
