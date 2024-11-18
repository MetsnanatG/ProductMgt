using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using webapi.AppDbContext.DTO;
using webapi.Models;

public class AuthService
{
    private readonly UserManager<User> _userManager;
    private readonly IConfiguration _configuration;
    private readonly SignInManager<User> _signInManager;

    public AuthService(UserManager<User> userManager, SignInManager<User> signInManager, IConfiguration configuration)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
    }

    public async Task<string> Authenticate(string username, string password)
    {
        // Find user by username
        var user = await _userManager.FindByNameAsync(username);
        if (user == null)
        {
            return null; // User not found
        }

        // Check password
        var result = await _signInManager.CheckPasswordSignInAsync(user, password, false);
        if (!result.Succeeded)
        {
            return null; // Invalid credentials
        }

        // Generate JWT token
        return GenerateJwtToken(user);
    }

    private string GenerateJwtToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName)
            }),
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }


    public async Task<IdentityResult> RegisterUser(RegisterDto registerDto)
    {
        var user = new User
        {
            UserName = registerDto.UserName,
            Email = registerDto.Email,
            NormalizedUserName = registerDto.UserName.ToUpper(),
            NormalizedEmail = registerDto.Email.ToUpper(),
            Role = registerDto.Role // Set the role if provided
        };

        var result = await _userManager.CreateAsync(user, registerDto.Password);
        return result;
    }

}
