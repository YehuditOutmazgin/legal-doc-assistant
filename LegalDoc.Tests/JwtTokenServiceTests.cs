using LegalDoc.Core.Enums;
using LegalDoc.Core.Interfaces;
using LegalDoc.Core.Models;
using LegalDoc.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;

namespace LegalDoc.Tests
{
    public class JwtTokenServiceTests
    {
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<IRefreshTokenRepository> _mockRefreshTokenRepository;
        private readonly JwtTokenService _tokenService;
        private const string ValidSecretKey = "this-is-a-super-secret-key-for-jwt-testing-that-is-at-least-32-characters-long";
        private const string ValidIssuer = "LegalDocAPI";
        private const string ValidAudience = "LegalDocClient";

        public JwtTokenServiceTests()
        {
            _mockConfiguration = new Mock<IConfiguration>();
            _mockRefreshTokenRepository = new Mock<IRefreshTokenRepository>();

            // Setup configuration
            _mockConfiguration
                .Setup(c => c["JwtSettings:SecretKey"])
                .Returns(ValidSecretKey);

            _mockConfiguration
                .Setup(c => c["JwtSettings:Issuer"])
                .Returns(ValidIssuer);

            _mockConfiguration
                .Setup(c => c["JwtSettings:Audience"])
                .Returns(ValidAudience);

            _mockConfiguration
                .Setup(c => c["JwtSettings:AccessTokenExpiryMinutes"])
                .Returns("15");

            _tokenService = new JwtTokenService(_mockConfiguration.Object, _mockRefreshTokenRepository.Object);
        }

        #region GenerateAccessToken Tests

        [Fact]
        public void GenerateAccessToken_WithValidUser_ReturnsValidJwtToken()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                Email = "user@example.com",
                FirstName = "John",
                LastName = "Doe",
                Role = UserRole.LAWYER,
                CreatedAt = DateTime.UtcNow
            };

            // Act
            var token = _tokenService.GenerateAccessToken(user);

            // Assert
            Assert.NotEmpty(token);
            Assert.NotNull(token);

            // Verify token structure
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadToken(token) as JwtSecurityToken;

            Assert.NotNull(jwtToken);
            Assert.Equal(ValidIssuer, jwtToken.Issuer);
            Assert.Equal(ValidAudience, jwtToken.Audiences.First());

            // Verify claims
            var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier);
            var emailClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Email);
            var roleClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role);
            var firstNameClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "firstName");
            var lastNameClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "lastName");

            Assert.NotNull(userIdClaim);
            Assert.Equal("1", userIdClaim.Value);
            Assert.NotNull(emailClaim);
            Assert.Equal("user@example.com", emailClaim.Value);
            Assert.NotNull(roleClaim);
            Assert.Equal("LAWYER", roleClaim.Value);
            Assert.NotNull(firstNameClaim);
            Assert.Equal("John", firstNameClaim.Value);
            Assert.NotNull(lastNameClaim);
            Assert.Equal("Doe", lastNameClaim.Value);
        }

        [Fact]
        public void GenerateAccessToken_WithDifferentUser_GeneratesDifferentTokens()
        {
            // Arrange
            var user1 = new User
            {
                Id = 1,
                Email = "user1@example.com",
                FirstName = "John",
                LastName = "Doe",
                Role = UserRole.LAWYER,
                CreatedAt = DateTime.UtcNow
            };

            var user2 = new User
            {
                Id = 2,
                Email = "user2@example.com",
                FirstName = "Jane",
                LastName = "Smith",
                Role = UserRole.ADMIN,
                CreatedAt = DateTime.UtcNow
            };

            // Act
            var token1 = _tokenService.GenerateAccessToken(user1);
            var token2 = _tokenService.GenerateAccessToken(user2);

            // Assert
            Assert.NotEqual(token1, token2);

            // Verify different claims
            var handler = new JwtSecurityTokenHandler();
            var jwt1 = handler.ReadToken(token1) as JwtSecurityToken;
            var jwt2 = handler.ReadToken(token2) as JwtSecurityToken;

            var user1IdClaim = jwt1?.Claims.First(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier).Value;
            var user2IdClaim = jwt2?.Claims.First(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier).Value;

            Assert.NotEqual(user1IdClaim, user2IdClaim);
        }

        [Fact]
        public void GenerateAccessToken_TokenHasCorrectExpiry()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                Email = "user@example.com",
                FirstName = "John",
                LastName = "Doe",
                Role = UserRole.LAWYER,
                CreatedAt = DateTime.UtcNow
            };

            var beforeGeneration = DateTime.UtcNow;

            // Act
            var token = _tokenService.GenerateAccessToken(user);

            var afterGeneration = DateTime.UtcNow;

            // Assert
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadToken(token) as JwtSecurityToken;

            Assert.NotNull(jwtToken?.ValidTo);
            // Token should expire in approximately 15 minutes
            var expectedExpiryMin = beforeGeneration.AddMinutes(14.5);
            var expectedExpiryMax = afterGeneration.AddMinutes(15.5);
            Assert.True(jwtToken!.ValidTo >= expectedExpiryMin && jwtToken.ValidTo <= expectedExpiryMax);
        }

        #endregion

        #region GenerateRefreshToken Tests

        [Fact]
        public void GenerateRefreshToken_ReturnsNonEmptyString()
        {
            // Act
            var token = _tokenService.GenerateRefreshToken();

            // Assert
            Assert.NotEmpty(token);
            Assert.NotNull(token);
        }

        [Fact]
        public void GenerateRefreshToken_GeneratesDifferentTokensEachCall()
        {
            // Act
            var token1 = _tokenService.GenerateRefreshToken();
            var token2 = _tokenService.GenerateRefreshToken();

            // Assert
            Assert.NotEqual(token1, token2);
        }

        [Fact]
        public void GenerateRefreshToken_TokenIsBase64Encoded()
        {
            // Act
            var token = _tokenService.GenerateRefreshToken();

            // Assert
            // If it's valid base64, this should not throw
            var decodedBytes = Convert.FromBase64String(token);
            Assert.NotEmpty(decodedBytes);
        }

        #endregion

        #region ValidateRefreshToken Tests

        [Fact]
        public async Task ValidateRefreshTokenAsync_WithValidToken_ReturnsUserId()
        {
            // Arrange
            var token = "valid_refresh_token";
            var userId = 1;
            var refreshToken = new RefreshToken
            {
                Token = token,
                UserId = userId,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                IsRevoked = false,
                CreatedAt = DateTime.UtcNow
            };

            _mockRefreshTokenRepository
                .Setup(r => r.GetByTokenAsync(token))
                .ReturnsAsync(refreshToken);

            // Act
            var result = await _tokenService.ValidateRefreshTokenAsync(token);

            // Assert
            Assert.Equal(userId, result);
            _mockRefreshTokenRepository.Verify(r => r.GetByTokenAsync(token), Times.Once);
        }

        [Fact]
        public async Task ValidateRefreshTokenAsync_WithNonExistentToken_ReturnsNull()
        {
            // Arrange
            var token = "nonexistent_token";

            _mockRefreshTokenRepository
                .Setup(r => r.GetByTokenAsync(token))
                .ReturnsAsync((RefreshToken?)null);

            // Act
            var result = await _tokenService.ValidateRefreshTokenAsync(token);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task ValidateRefreshTokenAsync_WithRevokedToken_ReturnsNull()
        {
            // Arrange
            var token = "revoked_token";
            var refreshToken = new RefreshToken
            {
                Token = token,
                UserId = 1,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                IsRevoked = true,
                CreatedAt = DateTime.UtcNow
            };

            _mockRefreshTokenRepository
                .Setup(r => r.GetByTokenAsync(token))
                .ReturnsAsync(refreshToken);

            // Act
            var result = await _tokenService.ValidateRefreshTokenAsync(token);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task ValidateRefreshTokenAsync_WithExpiredToken_ReturnsNullAndRevokesToken()
        {
            // Arrange
            var token = "expired_token";
            var refreshToken = new RefreshToken
            {
                Token = token,
                UserId = 1,
                ExpiresAt = DateTime.UtcNow.AddDays(-1), // Expired yesterday
                IsRevoked = false,
                CreatedAt = DateTime.UtcNow.AddDays(-8)
            };

            _mockRefreshTokenRepository
                .Setup(r => r.GetByTokenAsync(token))
                .ReturnsAsync(refreshToken);

            _mockRefreshTokenRepository
                .Setup(r => r.RevokeTokenAsync(token))
                .ReturnsAsync(true);

            // Act
            var result = await _tokenService.ValidateRefreshTokenAsync(token);

            // Assert
            Assert.Null(result);
            _mockRefreshTokenRepository.Verify(r => r.RevokeTokenAsync(token), Times.Once);
        }

        #endregion

        #region StoreRefreshToken Tests

        [Fact]
        public async Task StoreRefreshTokenAsync_WithValidData_CreatesRefreshToken()
        {
            // Arrange
            var token = "new_refresh_token";
            var userId = 1;
            var expiry = DateTime.UtcNow.AddDays(7);

            var storedToken = new RefreshToken
            {
                Token = token,
                UserId = userId,
                ExpiresAt = expiry,
                IsRevoked = false,
                CreatedAt = DateTime.UtcNow
            };

            _mockRefreshTokenRepository
                .Setup(r => r.CreateAsync(It.IsAny<RefreshToken>()))
                .ReturnsAsync(storedToken);

            // Act
            await _tokenService.StoreRefreshTokenAsync(token, userId, expiry);

            // Assert
            _mockRefreshTokenRepository.Verify(
                r => r.CreateAsync(It.Is<RefreshToken>(rt =>
                    rt.Token == token &&
                    rt.UserId == userId &&
                    rt.IsRevoked == false)),
                Times.Once);
        }

        #endregion

        #region RevokeRefreshToken Tests

        [Fact]
        public async Task RevokeRefreshTokenAsync_WithValidToken_CallsRepository()
        {
            // Arrange
            var token = "token_to_revoke";

            _mockRefreshTokenRepository
                .Setup(r => r.RevokeTokenAsync(token))
                .ReturnsAsync(true);

            // Act
            await _tokenService.RevokeRefreshTokenAsync(token);

            // Assert
            _mockRefreshTokenRepository.Verify(r => r.RevokeTokenAsync(token), Times.Once);
        }

        #endregion
    }
}
