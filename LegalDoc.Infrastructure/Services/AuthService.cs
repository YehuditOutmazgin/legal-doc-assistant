using LegalDoc.Core.DTOs;
using LegalDoc.Core.Interfaces;
using LegalDoc.Core.Models;
using Microsoft.Extensions.Configuration;

namespace LegalDoc.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly ITokenService _tokenService;
        private readonly IConfiguration _config;

        public AuthService(
            IUserRepository userRepository,
            ITokenService tokenService,
            IConfiguration config)
        {
            _userRepository = userRepository;
            _tokenService = tokenService;
            _config = config;
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
        {
            var user = await _userRepository.GetByEmailAsync(loginDto.Email)
                ?? throw new UnauthorizedAccessException("Invalid email or password.");

            if (!user.IsActive)
                throw new UnauthorizedAccessException("Account is deactivated.");

            if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
                throw new UnauthorizedAccessException("Invalid email or password.");

            return BuildAuthResponse(user);
        }

        public async Task<AuthResponseDto> RefreshTokenAsync(string refreshToken)
        {
            var userId = _tokenService.ValidateRefreshToken(refreshToken)
                ?? throw new UnauthorizedAccessException("Invalid or expired refresh token.");

            var user = await _userRepository.GetByIdAsync(userId)
                ?? throw new UnauthorizedAccessException("User not found.");

            _tokenService.RevokeRefreshToken(refreshToken);
            return BuildAuthResponse(user);
        }

        public Task LogoutAsync(string refreshToken)
        {
            _tokenService.RevokeRefreshToken(refreshToken);
            return Task.CompletedTask;
        }

        public async Task<UserDto> RegisterAsync(RegisterDto registerDto)
        {
            if (await _userRepository.EmailExistsAsync(registerDto.Email))
                throw new InvalidOperationException("Email already in use.");

            var user = new User
            {
                Email = registerDto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                Role = registerDto.Role,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            var created = await _userRepository.CreateAsync(user);
            return MapToUserDto(created);
        }

        private AuthResponseDto BuildAuthResponse(User user)
        {
            var accessToken = _tokenService.GenerateAccessToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken();
            var expiry = DateTime.UtcNow.AddDays(
                int.Parse(_config["JwtSettings:RefreshTokenExpiryDays"]!));

            _tokenService.StoreRefreshToken(refreshToken, user.Id, expiry);

            return new AuthResponseDto
            {
                Token = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(
                    int.Parse(_config["JwtSettings:AccessTokenExpiryMinutes"]!)),
                User = MapToUserDto(user)
            };
        }

        private static UserDto MapToUserDto(User user) => new()
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = user.Role,
            CreatedAt = user.CreatedAt,
            IsActive = user.IsActive
        };
    }
}
