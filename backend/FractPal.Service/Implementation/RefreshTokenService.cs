using FractPal.Service.Interface;
using FractPal.Model.Entity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace FractPal.Service.Implementation
{
    public class RefreshTokenService : IRefreshTokenService
    {
        private IRepository<RefreshToken> _refreshTokenRepository;
        public RefreshTokenService(IRepository<RefreshToken> refreshTokenRepository)
        {
            _refreshTokenRepository = refreshTokenRepository;
        }

        public async Task<string> GenerateRefreshToken(IdentityUser user)
        {
            string token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            RefreshToken refreshToken = new RefreshToken()
            {
                Token = token,
                UserId = Guid.Parse(user.Id),
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                IsRevoked = false
            };

            await _refreshTokenRepository.AddAsync(refreshToken);
            await _refreshTokenRepository.CommitAsync();
            return token;
        }

        public async Task<Guid?> GetUserIdByRefreshToken(string refreshToken)
        {
            var token = await _refreshTokenRepository.Query()
                .FirstOrDefaultAsync(rt => rt.Token == refreshToken && rt.IsRevoked == false);
            if(token != null)
            {
                return token.UserId;
            }

            return null;
        }

        public async Task InvalidateRefreshToken(string refreshToken)
        {
            RefreshToken? token = await _refreshTokenRepository.Query()
                .FirstOrDefaultAsync(rt => rt.Token == refreshToken);
            if(token != null)
            {
                token.IsRevoked = true;
                await _refreshTokenRepository.CommitAsync();
            }
        }

        public async Task<bool> ValidateRefreshToken(Guid userId, string refreshToken)
        {
            var token = await _refreshTokenRepository.Query()
               .FirstOrDefaultAsync(rt => rt.Token == refreshToken && rt.IsRevoked == false && rt.UserId == userId);
            return token != null;
        }
    }
}
