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

        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var token = _authService.GenerateToken(user);
        return Ok(new AuthResponse(token, user.Username, user.Email));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _context.Users.SingleOrDefaultAsync(u => u.Username == request.Username);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return Unauthorized("Invalid credentials.");
        }

        var token = _authService.GenerateToken(user);
        return Ok(new AuthResponse(token, user.Username, user.Email));
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return NotFound();

        var activeDecorations = await _context.UserDecorations
            .Include(ud => ud.Decoration)
            .Where(ud => ud.UserId == userId && ud.IsEquipped)
            .Select(ud => new { ud.Decoration.Type, ud.Decoration.ImageUrl })
            .ToListAsync();

        return Ok(new 
        { 
            user.Id, 
            user.Username, 
            user.Email, 
            user.GachaPoints,
            user.LastDrawDate,
            Equipped = activeDecorations
        });
    }
}
