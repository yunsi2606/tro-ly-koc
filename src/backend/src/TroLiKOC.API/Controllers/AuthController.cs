using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TroLiKOC.Modules.Identity.Infrastructure;
using TroLiKOC.Modules.Identity.Domain.Entities;

namespace TroLiKOC.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IdentityDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthController(IdentityDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        // Check if email already exists
        var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (existingUser != null)
        {
            return BadRequest(new { error = "Email đã được sử dụng" });
        }

        // Create new user using IdentityUser constructor
        var user = new User(request.Email, request.Email, request.Name)
        {
            PasswordHash = HashPassword(request.Password),
            EmailConfirmed = true
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Generate JWT token
        var token = GenerateJwtToken(user);

        return Ok(new
        {
            token,
            user = new
            {
                id = user.Id,
                email = user.Email,
                name = user.DisplayName,
                createdAt = user.CreatedAt
            }
        });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (user == null || !VerifyPassword(request.Password, user.PasswordHash ?? ""))
        {
            return Unauthorized(new { error = "Email hoặc mật khẩu không đúng" });
        }

        var token = GenerateJwtToken(user);

        return Ok(new
        {
            token,
            user = new
            {
                id = user.Id,
                email = user.Email,
                name = user.DisplayName,
                createdAt = user.CreatedAt
            }
        });
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var user = await _context.Users.FindAsync(Guid.Parse(userId));
        if (user == null)
        {
            return NotFound();
        }

        return Ok(new
        {
            id = user.Id,
            email = user.Email,
            name = user.DisplayName,
            createdAt = user.CreatedAt
        });
    }

    private string GenerateJwtToken(User user)
    {
        var secretKey = _configuration["Jwt:SecretKey"] ?? "TroLiKOC_SuperSecretKey_2026_MinLength32Chars!";
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email ?? ""),
            new Claim(ClaimTypes.Name, user.DisplayName)
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"] ?? "TroLiKOC",
            audience: _configuration["Jwt:Audience"] ?? "TroLiKOC",
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string HashPassword(string password)
    {
        // In production, use BCrypt or Argon2
        return Convert.ToBase64String(
            System.Security.Cryptography.SHA256.HashData(
                Encoding.UTF8.GetBytes(password + "TroLiKOC_Salt")
            )
        );
    }

    private static bool VerifyPassword(string password, string hash)
    {
        return HashPassword(password) == hash;
    }
}

public class RegisterRequest
{
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
}

public class LoginRequest
{
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
}
