using BCrypt.Net;
using FortuneGacha.Api.Data;
using FortuneGacha.Api.DTOs;
using FortuneGacha.Api.Models;
using FortuneGacha.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace FortuneGacha.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly GachaDbContext _context;
    private readonly IAuthService _authService;

    public AuthController(GachaDbContext context, IAuthService authService)
    {
        _context = context;
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (await _context.Users.AnyAsync(u => u.Username == request.Username || u.Email == request.Email))
        {
            return BadRequest("Username or Email already exists.");
        }

        var profile = new GachaProfile
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
        };

        _context.Users.Add(profile);
        await _context.SaveChangesAsync();

        var token = _authService.GenerateToken(profile);
        return Ok(new AuthResponse(token, profile.Username, profile.Email));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var profile = await _context.Users.SingleOrDefaultAsync(u => u.Username == request.Username);

        if (profile == null || !BCrypt.Net.BCrypt.Verify(request.Password, profile.PasswordHash))
        {
            return Unauthorized("Invalid credentials.");
        }

        var token = _authService.GenerateToken(profile);
        return Ok(new AuthResponse(token, profile.Username, profile.Email));
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUser()
    {
        var idClaim = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(idClaim)) return Unauthorized();

        var userId = int.Parse(idClaim);
        var profile = await _context.Users.FindAsync(userId);
        if (profile == null) return NotFound();

        var activeDecorations = await _context.UserDecorations
            .Include(ud => ud.Decoration)
            .Where(ud => ud.UserId == userId && ud.IsEquipped)
            .Select(ud => new { ud.Decoration.Type, ud.Decoration.ImageUrl })
            .ToListAsync();

        return Ok(new 
        { 
            profile.Id, 
            profile.Username, 
            profile.Email, 
            profile.GachaPoints,
            profile.LastDrawDate,
            profile.ZodiacSign,
            profile.PersonalInterests,
            profile.Bio,
            Equipped = activeDecorations
        });
    }

    [HttpPut("update")]
    [Authorize]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var idClaim = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(idClaim)) return Unauthorized();

        var userId = int.Parse(idClaim);
        var profile = await _context.Users.FindAsync(userId);
        if (profile == null) return NotFound();

        profile.ZodiacSign = request.ZodiacSign;
        profile.PersonalInterests = request.PersonalInterests;
        profile.Bio = request.Bio;
        profile.BirthDate = request.BirthDate;

        await _context.SaveChangesAsync();
        return Ok(profile);
    }
}
