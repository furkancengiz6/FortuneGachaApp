using System.Security.Claims;
using FortuneGacha.Api.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FortuneGacha.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationController : ControllerBase
{
    private readonly GachaDbContext _context;

    public NotificationController(GachaDbContext context)
    {
        _context = context;
    }

    [HttpPost("register-token")]
    public async Task<IActionResult> RegisterToken([FromBody] string pushToken)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdString == null) return Unauthorized();

        var userId = int.Parse(userIdString);
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return NotFound();

        user.PushToken = pushToken;
        await _context.SaveChangesAsync();

        return Ok(new { Message = "Push token registered successfully." });
    }
}
