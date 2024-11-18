using Microsoft.AspNetCore.Mvc;
using webapi.AppDbContext.DTO;
using webapi.Models;
using webapi.Service;

namespace webapi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            // Authenticate the user and return JWT token
            var token = await _authService.Authenticate(loginDto.UserName, loginDto.Password);

            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized("Invalid credentials.");
            }

            return Ok(new { Token = token });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            var result = await _authService.RegisterUser(registerDto);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return Ok("User registered successfully.");
        }

    }
}
