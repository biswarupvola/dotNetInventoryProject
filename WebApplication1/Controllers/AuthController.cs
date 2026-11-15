using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using WebApplication1.Models;
using WebApplication1.data;

namespace WebApplication1.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly IConfiguration _configuration;

    public AuthController(
        AppDbContext context,
        IPasswordHasher<User> passwordHasher,
        IConfiguration configuration)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _configuration = configuration;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthenticationResponse>> Register(RegisterRequest request)
    {
        var username = request.Username.Trim();
        var email = request.Email.Trim().ToLowerInvariant();

        if (await _context.Users.AnyAsync(user => user.Username == username || user.Email == email))
        {
            return Conflict(new { message = "Username or email is already registered." });
        }

        var user = new User
        {
            Username = username,
            Email = email,
            PasswordHash = string.Empty
        };
        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return Ok(CreateAuthenticationResponse(user));
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthenticationResponse>> Login(LoginRequest request)
    {
        var identifier = request.UsernameOrEmail.Trim();
        var normalizedEmail = identifier.ToLowerInvariant();
        var user = await _context.Users.SingleOrDefaultAsync(candidate =>
            candidate.Username == identifier || candidate.Email == normalizedEmail);

        if (user is null || _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password)
            == PasswordVerificationResult.Failed)
        {
            return Unauthorized(new { message = "Invalid username/email or password." });
        }

        return Ok(CreateAuthenticationResponse(user));
    }

    private AuthenticationResponse CreateAuthenticationResponse(User user)
    {
        var expiresAtUtc = DateTime.UtcNow.AddMinutes(
            _configuration.GetValue<int>("Jwt:ExpiresInMinutes", 60));
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username)
        };
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            _configuration["Jwt:Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: expiresAtUtc,
            signingCredentials: credentials);

        return new AuthenticationResponse(
            user.Id,
            user.Username,
            user.Email,
            new JwtSecurityTokenHandler().WriteToken(token),
            expiresAtUtc);
    }
}
