using FractPal.Model.DTO.Auth;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foodie.Business.Services.Interfaces;

public interface IRefreshTokenService
{
    Task<string> GenerateRefreshToken(IdentityUser user);

    Task<bool> ValidateRefreshToken(Guid userId, string refreshToken);

    Task InvalidateRefreshToken(string refreshToken);

    Task<Guid?> GetUserIdByRefreshToken(string refreshToken);
}
