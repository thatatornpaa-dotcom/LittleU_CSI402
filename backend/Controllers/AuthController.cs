using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using backend.Data;
using backend.Models;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;

    public AuthController(AppDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    public record RegisterRequest(string Email, string Password, string? FirstName, string? LastName, string? Phone);
    public record LoginRequest(string Email, string Password);

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Password))
            return BadRequest(new { message = "กรุณากรอกอีเมลและรหัสผ่าน" });

        var emailNorm = req.Email.Trim().ToLower();
        var exists = await _db.Users.AnyAsync(u => u.Email.ToLower() == emailNorm);
        if (exists)
            return Conflict(new { message = "อีเมลนี้ถูกใช้งานแล้ว" });

        var user = new User
        {
            Email = emailNorm,
            Password = req.Password,
            FirstName = req.FirstName,
            LastName = req.LastName,
            Phone = req.Phone,
            Role = "BUYER",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        var token = GenerateJwt(user);
        return Ok(new
        {
            message = "สมัครสมาชิกสำเร็จ",
            token,
            user = new { user.Id, user.Email, user.FirstName, user.LastName, user.Phone, user.Role }
        });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Password))
            return BadRequest(new { message = "กรุณากรอกอีเมลและรหัสผ่าน" });

        var emailNorm = req.Email.Trim().ToLower();
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == emailNorm);

        if (user == null)
            return Unauthorized(new { message = "ไม่พบบัญชีผู้ใช้นี้" });

        // รองรับทั้งรหัสผ่านตรงและรหัสผ่านที่ hash ไว้ในระบบเดิม
        var validPassword = user.Password == req.Password ||
                            (user.Password.StartsWith("") && BCrypt.Net.BCrypt.Verify(req.Password, user.Password));

        if (!validPassword)
            return Unauthorized(new { message = "รหัสผ่านไม่ถูกต้อง" });

        var token = GenerateJwt(user);
        return Ok(new
        {
            message = "เข้าสู่ระบบสำเร็จ",
            token,
            user = new { user.Id, user.Email, user.FirstName, user.LastName, user.Phone, user.Address, user.Role }
        });
    }

    [Authorize]
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var user = await _db.Users
            .Include(u => u.Addresses)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null) return NotFound(new { message = "ไม่พบข้อมูลผู้ใช้" });

        return Ok(new
        {
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            user.Phone,
            user.Address,
            user.Role,
            addresses = user.Addresses.OrderByDescending(a => a.IsDefault)
        });
    }

    private string GenerateJwt(User user)
    {
        var jwtKey = _config["Jwt:Key"] ?? "LittleU_CSI402_Fallback_Secret_Key_2026!";
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
