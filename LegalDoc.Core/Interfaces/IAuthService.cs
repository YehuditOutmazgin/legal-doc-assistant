using LegalDoc.Core.DTOs;

namespace LegalDoc.Core.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> LoginAsync(LoginDto loginDto);
        Task<AuthResponseDto> RefreshTokenAsync(string refreshToken);
        Task LogoutAsync(string refreshToken);
        Task<UserDto> RegisterAsync(RegisterDto registerDto);
    }
}
