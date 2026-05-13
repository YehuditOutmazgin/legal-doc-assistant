using LegalDoc.Core.DTOs;
using LegalDoc.Core.Enums;
using LegalDoc.Core.Interfaces;
using LegalDoc.Core.Models;
using LegalDoc.Infrastructure.Services;
using Microsoft.Extensions.Configuration;

namespace LegalDoc.Tests
{
    public class AuthServiceTests
    {
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<ITokenService> _mockTokenService;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _mockTokenService = new Mock<ITokenService>();
            _mockConfiguration = new Mock<IConfiguration>();
            _authService = new AuthService(_mockUserRepository.Object, _mockTokenService.Object, _mockConfiguration.Object);
        }

        #region Login Tests

        [Fact]
        public async Task LoginAsync_WithValidCredentials_ReturnsAuthResponseWithToken()
        {
            // Arrange
            var loginDto = new LoginDto
            {
                Email = "user@example.com",
                Password = "password123"
            };

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword("password123");
            var user = new User
            {
                Id = 1,
                Email = "user@example.com",
                PasswordHash = hashedPassword,
                FirstName = "John",
                LastName = "Doe",
                Role = UserRole.LAWYER,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _mockUserRepository
                .Setup(r => r.GetByEmailAsync(loginDto.Email))
                .ReturnsAsync(user);

            _mockTokenService
                .Setup(t => t.GenerateAccessToken(user))
                .Returns("access_token_123");

            _mockTokenService
                .Setup(t => t.GenerateRefreshToken())
                .Returns("refresh_token_123");

            _mockConfiguration
                .Setup(c => c["JwtSettings:RefreshTokenExpiryDays"])
                .Returns("7");

            _mockConfiguration
                .Setup(c => c["JwtSettings:AccessTokenExpiryMinutes"])
                .Returns("15");

            // Act
            var result = await _authService.LoginAsync(loginDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("access_token_123", result.Token);
            Assert.Equal("John", result.User.FirstName);
            Assert.Equal("Doe", result.User.LastName);
            Assert.Equal("user@example.com", result.User.Email);
            _mockUserRepository.Verify(r => r.GetByEmailAsync(loginDto.Email), Times.Once);
        }

        [Fact]
        public async Task LoginAsync_WithInvalidPassword_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var loginDto = new LoginDto
            {
                Email = "user@example.com",
                Password = "wrongpassword"
            };

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword("correctpassword");
            var user = new User
            {
                Id = 1,
                Email = "user@example.com",
                PasswordHash = hashedPassword,
                FirstName = "John",
                LastName = "Doe",
                Role = UserRole.LAWYER,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _mockUserRepository
                .Setup(r => r.GetByEmailAsync(loginDto.Email))
                .ReturnsAsync(user);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _authService.LoginAsync(loginDto));
            Assert.Equal("Invalid email or password.", ex.Message);
        }

        [Fact]
        public async Task LoginAsync_WithNonExistentUser_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var loginDto = new LoginDto
            {
                Email = "nonexistent@example.com",
                Password = "password123"
            };

            _mockUserRepository
                .Setup(r => r.GetByEmailAsync(loginDto.Email))
                .ReturnsAsync((User?)null);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _authService.LoginAsync(loginDto));
            Assert.Equal("Invalid email or password.", ex.Message);
        }

        [Fact]
        public async Task LoginAsync_WithInactiveUser_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var loginDto = new LoginDto
            {
                Email = "user@example.com",
                Password = "password123"
            };

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword("password123");
            var user = new User
            {
                Id = 1,
                Email = "user@example.com",
                PasswordHash = hashedPassword,
                FirstName = "John",
                LastName = "Doe",
                Role = UserRole.LAWYER,
                IsActive = false,
                CreatedAt = DateTime.UtcNow
            };

            _mockUserRepository
                .Setup(r => r.GetByEmailAsync(loginDto.Email))
                .ReturnsAsync(user);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _authService.LoginAsync(loginDto));
            Assert.Equal("Account is deactivated.", ex.Message);
        }

        #endregion

        #region Register Tests

        [Fact]
        public async Task RegisterAsync_WithValidData_ReturnsUserDtoWithId()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                Email = "newuser@example.com",
                Password = "password123",
                FirstName = "Jane",
                LastName = "Smith",
                Role = UserRole.LAWYER
            };

            var createdUser = new User
            {
                Id = 2,
                Email = registerDto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                Role = registerDto.Role,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _mockUserRepository
                .Setup(r => r.EmailExistsAsync(registerDto.Email))
                .ReturnsAsync(false);

            _mockUserRepository
                .Setup(r => r.CreateAsync(It.IsAny<User>()))
                .ReturnsAsync(createdUser);

            // Act
            var result = await _authService.RegisterAsync(registerDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Id);
            Assert.Equal("Jane", result.FirstName);
            Assert.Equal("Smith", result.LastName);
            Assert.Equal("newuser@example.com", result.Email);
            Assert.True(result.IsActive);
            _mockUserRepository.Verify(r => r.CreateAsync(It.IsAny<User>()), Times.Once);
        }

        [Fact]
        public async Task RegisterAsync_WithExistingEmail_ThrowsInvalidOperationException()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                Email = "existing@example.com",
                Password = "password123",
                FirstName = "Jane",
                LastName = "Smith",
                Role = UserRole.LAWYER
            };

            _mockUserRepository
                .Setup(r => r.EmailExistsAsync(registerDto.Email))
                .ReturnsAsync(true);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _authService.RegisterAsync(registerDto));
            Assert.Equal("Email already in use.", ex.Message);
            _mockUserRepository.Verify(r => r.CreateAsync(It.IsAny<User>()), Times.Never);
        }

        #endregion

        #region RefreshToken Tests

        [Fact]
        public async Task RefreshTokenAsync_WithValidToken_ReturnsNewAuthResponse()
        {
            // Arrange
            var refreshToken = "valid_refresh_token";
            var userId = 1;
            var user = new User
            {
                Id = userId,
                Email = "user@example.com",
                PasswordHash = "hashed",
                FirstName = "John",
                LastName = "Doe",
                Role = UserRole.LAWYER,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _mockTokenService
                .Setup(t => t.ValidateRefreshToken(refreshToken))
                .Returns(userId);

            _mockUserRepository
                .Setup(r => r.GetByIdAsync(userId))
                .ReturnsAsync(user);

            _mockTokenService
                .Setup(t => t.GenerateAccessToken(user))
                .Returns("new_access_token");

            _mockTokenService
                .Setup(t => t.GenerateRefreshToken())
                .Returns("new_refresh_token");

            _mockConfiguration
                .Setup(c => c["JwtSettings:RefreshTokenExpiryDays"])
                .Returns("7");

            _mockConfiguration
                .Setup(c => c["JwtSettings:AccessTokenExpiryMinutes"])
                .Returns("15");

            // Act
            var result = await _authService.RefreshTokenAsync(refreshToken);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("new_access_token", result.Token);
            Assert.Equal(userId, result.User.Id);
            _mockTokenService.Verify(t => t.RevokeRefreshToken(refreshToken), Times.Once);
        }

        [Fact]
        public async Task RefreshTokenAsync_WithInvalidToken_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var refreshToken = "invalid_refresh_token";

            _mockTokenService
                .Setup(t => t.ValidateRefreshToken(refreshToken))
                .Returns((int?)null);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _authService.RefreshTokenAsync(refreshToken));
            Assert.Equal("Invalid or expired refresh token.", ex.Message);
        }

        [Fact]
        public async Task RefreshTokenAsync_WithNonExistentUser_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var refreshToken = "valid_refresh_token";
            var userId = 999;

            _mockTokenService
                .Setup(t => t.ValidateRefreshToken(refreshToken))
                .Returns(userId);

            _mockUserRepository
                .Setup(r => r.GetByIdAsync(userId))
                .ReturnsAsync((User?)null);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _authService.RefreshTokenAsync(refreshToken));
            Assert.Equal("User not found.", ex.Message);
        }

        #endregion

        #region Logout Tests

        [Fact]
        public async Task LogoutAsync_WithValidToken_CallsRevokeRefreshToken()
        {
            // Arrange
            var refreshToken = "valid_refresh_token";

            // Act
            await _authService.LogoutAsync(refreshToken);

            // Assert
            _mockTokenService.Verify(t => t.RevokeRefreshToken(refreshToken), Times.Once);
        }

        #endregion
    }
}
