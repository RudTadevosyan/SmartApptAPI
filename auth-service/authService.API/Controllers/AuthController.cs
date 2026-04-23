using authService.Application.Interfaces;
using AuthService.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace authService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthJwtService _authService;

        public AuthController(IAuthJwtService authService)
        {
            _authService = authService;
        }

        [HttpPost("register/business")]
        public async Task<IActionResult> RegisterBusiness([FromBody] RegisterDto registerDto)
        {
            var tokens = await _authService.RegisterAsync(registerDto, "BusinessOwner");

            Response.Cookies.Append("refreshToken", tokens.RefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddDays(14),
                Path = "/"
            });

            return Ok(new { jwt = tokens.Jwt });
        }

        [HttpPost("register/customer")]
        public async Task<IActionResult> RegisterCustomer([FromBody] RegisterDto registerDto)
        {
            var tokens = await _authService.RegisterAsync(registerDto, "Customer");

            Response.Cookies.Append("refreshToken", tokens.RefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddDays(14),
                Path = "/"
            });

            return Ok(new { jwt = tokens.Jwt });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var tokens = await _authService.LoginAsync(loginDto);

            Response.Cookies.Append("refreshToken", tokens.RefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddDays(14),
                Path = "/"
            });

            return Ok(new { jwt = tokens.Jwt });
        }

        [HttpDelete("delete")]
        public async Task<IActionResult> Delete([FromBody] DeleteDto deleteUserDto)
        {
            await _authService.DeleteAsync(deleteUserDto);
            return Ok(new { message = "User deleted successfully." });
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh()
        {
            var refreshToken = Request.Cookies["refreshToken"];

            if (string.IsNullOrEmpty(refreshToken))
                return Unauthorized(new { message = "Refresh token missing." });

            var tokens = await _authService.RefreshAsync(refreshToken);

            Response.Cookies.Append("refreshToken", tokens.RefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddDays(14),
                Path = "/"
            });

            return Ok(new { jwt = tokens.Jwt });
        }

        [HttpPut("change_password")]
        public async Task<IActionResult> ChangePassword([FromBody] UpdateDto updateDto)
        {
            await _authService.ChangePasswordAsync(updateDto);
            return Ok();
        }

        [Authorize]
        [HttpGet("test")]
        public IActionResult TestAuth()
        {
            return Ok(new { message = "Test auth successful." });
        }

        [Authorize(Roles = "BusinessOwner")]
        [HttpGet("test_business")]
        public IActionResult TestBusiness()
        {
            return Ok(new { message = "Business auth successful." });
        }
    }
}