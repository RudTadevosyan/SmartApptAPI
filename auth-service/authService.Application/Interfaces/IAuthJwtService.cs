using AuthService.Shared.DTOs;

namespace authService.Application.Interfaces
{
    public interface IAuthJwtService
    {
        Task<JwtDto> LoginAsync(LoginDto loginDto);
        Task<JwtDto> RegisterAsync(RegisterDto registerDto, string role);
        Task<JwtDto> RefreshAsync(string refreshToken);
        Task DeleteAsync(DeleteDto deleteDto);
        Task ChangePasswordAsync(UpdateDto updateDto);
    }
}
