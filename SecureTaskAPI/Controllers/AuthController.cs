using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _configuration;

    public AuthController(AppDbContext db, IConfiguration configuration)
    {
        _db = db;
        _configuration = configuration;
    }

    // POST /auth/register
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (await _db.Users.AnyAsync(u => u.Username == request.Username))
            return BadRequest("Brugernavnet er allerede taget!");

        var newUser = new User
        {
            Username = request.Username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
        };

        _db.Users.Add(newUser);
        await _db.SaveChangesAsync();
        return Ok("Bruger oprettet!");
    }

    // POST /auth/login
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] RegisterRequest request)
    {
        var foundUser = await _db.Users
            .FirstOrDefaultAsync(u => u.Username == request.Username);

        if (foundUser is null || !BCrypt.Net.BCrypt.Verify(request.Password, foundUser.PasswordHash))
            return Unauthorized();

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, foundUser.Username),
            new Claim(ClaimTypes.NameIdentifier, foundUser.Id.ToString())
        };

        var jwtSecret = _configuration["JWT_SECRET"]!;
        var jwtIssuer = _configuration["JWT_ISSUER"]!;
        var jwtAudience = _configuration["JWT_AUDIENCE"]!;

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds
        );

        return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });
    }
}