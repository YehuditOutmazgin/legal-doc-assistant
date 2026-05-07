using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using LegalDoc.Core.Interfaces;
using LegalDoc.Core.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace LegalDoc.Infrastructure.Services
{
    public class JwtTokenService : ITokenService
    {
        private readonly IConfiguration _config;
        private readonly IRefreshTokenRepository _refreshTokenRepository;

        public JwtTokenService(IConfiguration config, IRefreshTokenRepository refreshTokenRepository)
        {
            _config = config;
            _refreshTokenRepository = refreshTokenRepository;
        }

        public string GenerateAccessToken(User user)
        {
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["JwtSettings:SecretKey"]!));

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim("firstName", user.FirstName),
                new Claim("lastName", user.LastName)
            };

            var token = new JwtSecurityToken(
                issuer: _config["JwtSettings:Issuer"],
                audience: _config["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(
                    int.Parse(_config["JwtSettings:AccessTokenExpiryMinutes"]!)),
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        }

        public async Task<int?> ValidateRefreshTokenAsync(string refreshToken)
        {
            var token = await _refreshTokenRepository.GetByTokenAsync(refreshToken);
            
            if (token == null || token.IsRevoked)
                return null;

            if (token.ExpiresAt <= DateTime.UtcNow)
            {
                await _refreshTokenRepository.RevokeTokenAsync(refreshToken);
                return null;
            }

            return token.UserId;
        }

        public async Task StoreRefreshTokenAsync(string token, int userId, DateTime expiry)
        {
            var refreshToken = new RefreshToken
            {
                Token = token,
                UserId = userId,
                ExpiresAt = expiry,
                CreatedAt = DateTime.UtcNow,
                IsRevoked = false
            };

            await _refreshTokenRepository.CreateAsync(refreshToken);
        }

        public async Task RevokeRefreshTokenAsync(string token)
        {
            await _refreshTokenRepository.RevokeTokenAsync(token);
        }

        // Legacy sync methods for backward compatibility
        public int? ValidateRefreshToken(string refreshToken)
        {
            return ValidateRefreshTokenAsync(refreshToken).GetAwaiter().GetResult();
        }

        public void StoreRefreshToken(string token, int userId, DateTime expiry)
        {
            StoreRefreshTokenAsync(token, userId, expiry).GetAwaiter().GetResult();
        }

        public void RevokeRefreshToken(string token)
        {
            RevokeRefreshTokenAsync(token).GetAwaiter().GetResult();
        }
    }
}
