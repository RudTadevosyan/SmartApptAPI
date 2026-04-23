using authService.Domain.Entities;
using AuthService.Shared.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using authService.Application.Interfaces;
using authService.Domain.CustomExceptions;
using authService.Infrastructure.Interfaces;

namespace authService.Application.Services
{
    public class AuthJwtService : IAuthJwtService
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IConfiguration _config;
        private readonly IRefreshTokenRepository _refreshTokenRepository;

        public AuthJwtService(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IConfiguration config,
            IRefreshTokenRepository refreshTokenRepository)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _config = config;
            _refreshTokenRepository = refreshTokenRepository;
        }

        public async Task<JwtDto> RegisterAsync(RegisterDto registerDto, string role)
        {
            var user = new User
            {
                FullName = registerDto.FullName,
                UserName = registerDto.UserName 
            };

            var result = await _userManager.CreateAsync(user, registerDto.Password);

            if (!result.Succeeded)
                throw new DomainException(string.Join(", ", result.Errors.Select(e => e.Description)));

            await _userManager.AddToRoleAsync(user, role);

            return await GenerateTokensAsync(user);
        }

        public async Task<JwtDto> LoginAsync(LoginDto loginDto)
        {
            var user = await _userManager.FindByNameAsync(loginDto.UserName)
                ?? throw new NotFoundException($"User {loginDto.UserName} not found");

            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);

            if (!result.Succeeded)
                throw new DomainException("Invalid password.");

            return await GenerateTokensAsync(user);
        }

        public async Task DeleteAsync(DeleteDto deleteDto)
        {
            var user = await _userManager.FindByNameAsync(deleteDto.UserName)
                ?? throw new NotFoundException($"User {deleteDto.UserName} not found");

            var passwordCheck = await _signInManager.CheckPasswordSignInAsync(user, deleteDto.Password, false);

            if (!passwordCheck.Succeeded)
                throw new DomainException("Invalid password.");

            var result = await _userManager.DeleteAsync(user);

            if (!result.Succeeded)
                throw new Exception("Failed to delete user.");
        }

        public async Task<JwtDto> RefreshAsync(string refreshToken)
        {
            var stored = await _refreshTokenRepository.GetValidTokenAsync(refreshToken);

            if (stored == null)
                throw new Exception("Invalid or expired refresh token.");

            await _refreshTokenRepository.RevokeAsync(stored);

            return await GenerateTokensAsync(stored.User);
        }

        public async Task ChangePasswordAsync(UpdateDto updateDto)
        {
            var user = await _userManager.FindByNameAsync(updateDto.UserName)
                ?? throw new NotFoundException($"User {updateDto.UserName} not found");

            var passwordCheck = await _signInManager.CheckPasswordSignInAsync(user, updateDto.Password, false);

            if (!passwordCheck.Succeeded)
                throw new DomainException("Invalid password.");

            var result = await _userManager.ChangePasswordAsync(
                user,
                updateDto.Password,
                updateDto.NewPassword
            );

            if (!result.Succeeded)
                throw new DomainException(string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        private async Task<JwtDto> GenerateTokensAsync(User user)
        {
            var accessToken = await GenerateJwt(user);

            var refreshToken = new RefreshToken
            {
                Token = Convert.ToBase64String(Guid.NewGuid().ToByteArray()),
                UserId = user.Id,
                CreatedAtUtc = DateTime.UtcNow,
                ExpiresAtUtc = DateTime.UtcNow.AddDays(14)
            };

            await _refreshTokenRepository.AddAsync(refreshToken);

            return new JwtDto(accessToken.Jwt, refreshToken.Token);
        }

        private async Task<JwtDto> GenerateJwt(User user)
        {
            var roles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            foreach (var role in roles)
                claims.Add(new Claim(ClaimTypes.Role, role));

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:Key"]!)
            );

            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials
            );

            return new JwtDto(
                new JwtSecurityTokenHandler().WriteToken(token),
                string.Empty
            );
        }
    }
}