using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Startup_IELTS.DTOs;
using Startup_IELTS.Models;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using YourNamespace.Helpers;

[ApiController]
[AllowAnonymous]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly StartupIeltsContext _db;
    private readonly IConfiguration _config;

    public AuthController(StartupIeltsContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        // Lấy userId từ JWT
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

        if (userIdClaim == null)
            return Unauthorized();

        Guid userId = Guid.Parse(userIdClaim.Value);

        var user = await _db.Users
            .Include(u => u.Role)
            .Where(u => u.Id == userId)
            .Select(u => new
            {
                u.Id,
                u.Email,
                u.FullName,
                Role = u.Role.Name
            })
            .FirstOrDefaultAsync();

        if (user == null)
            return Unauthorized();

        return Ok(user);
    }
    [AllowAnonymous]
    [HttpPost("login")]
    public IActionResult Login(LoginDto dto)
    {
        try
        {
            var user = _db.Users
                .Include(u => u.Role)
                .FirstOrDefault(u => u.Email == dto.Email);
            if (user == null) return Unauthorized();

            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return Unauthorized();

            var token = JwtHelper.GenerateToken(user, _config);
            return Ok(new
            {
                status = true,
                token,
                user = new
                {
                    id = user.Role.Id,
                    fullname = user.FullName,
                    role = user.Role.Name
                }
            });

        }
        catch (Exception ex)
        {
            return BadRequest(new { status = false, message = ex.Message });
        }
        
    }
    [AllowAnonymous]
    [HttpPost("register")]
    public IActionResult Register(RegisterDto dto)
    {   
        try
        {
            if (_db.Users.Any(x => x.Email == dto.Email))
                return BadRequest("Email already in use.");
            var user = new User
            {
                FullName = dto.Name,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                RoleId = 4,
                CreatedAt = DateTime.UtcNow
            };
            _db.Users.Add(user);
            _db.SaveChanges();
            return Ok(new { status = true, message = "Đăng ký thành công" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { status = false, message = ex.Message });
        }
        
    }
    
}
