using System.Security.Claims;
using FortuneGacha.Api.Data;
using FortuneGacha.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using FortuneGacha.Api.Hubs;
using FortuneGacha.Api.Services;

namespace FortuneGacha.Api.Controllers;

[ApiController]
[Route("api/social")] // Route remains same, only class name changes
[Authorize]
public class SocialHubController : ControllerBase
{
    private readonly GachaDbContext _context;
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly INotificationService _notificationService;

    public SocialHubController(GachaDbContext context, IHubContext<NotificationHub> hubContext, INotificationService notificationService)
    {
        _context = context;
        _hubContext = hubContext;
        _notificationService = notificationService;
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string q)
    {
        var currentUserIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentUserIdString == null) return Unauthorized();
        var currentUserId = int.Parse(currentUserIdString);

        var users = await _context.Users
            .Where(u => u.Id != currentUserId && u.Username.Contains(q))
            .Select(u => new { u.Id, u.Username })
            .Take(10)
            .ToListAsync();

        return Ok(users);
    }

    [HttpPost("request/{userId}")]
    public async Task<IActionResult> SendRequest(int userId)
    {
        var currentUserIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentUserIdString == null) return Unauthorized();
        var currentUserId = int.Parse(currentUserIdString);

        if (userId == currentUserId) return BadRequest("Kendine arkadaşlık isteği gönderemezsin.");

        if (await _context.Friendships.AnyAsync(f => 
            (f.RequesterId == currentUserId && f.ReceiverId == userId) || 
            (f.RequesterId == userId && f.ReceiverId == currentUserId)))
        {
            return BadRequest("Zaten bir arkadaşlık süreci mevcut.");
        }

        var friendship = new Friendship
        {
            RequesterId = currentUserId,
            ReceiverId = userId,
            Status = "Pending"
        };

        _context.Friendships.Add(friendship);
        await _context.SaveChangesAsync();

        var senderUser = await _context.Users.FindAsync(currentUserId);
        var receiverUser = await _context.Users.FindAsync(userId);

        await _hubContext.Clients.User(userId.ToString()).SendAsync("ReceiveNotification", new 
        { 
            title = "Yeni Arkadaşlık İsteği", 
            message = $"{senderUser?.Username} sana bir arkadaşlık isteği gönderdi!" 
        });

        if (receiverUser != null && !string.IsNullOrEmpty(receiverUser.PushToken))
        {
            await _notificationService.SendPushNotificationAsync(
                receiverUser.PushToken, 
                "Yeni Arkadaşlık İsteği", 
                $"{senderUser?.Username} sana bir arkadaşlık isteği gönderdi!"
            );
        }

        return Ok(new { Message = "Arkadaşlık isteği gönderildi." });
    }

    [HttpPost("respond/{requestId}")]
    public async Task<IActionResult> RespondRequest(int requestId, [FromQuery] bool accept)
    {
        var currentUserIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentUserIdString == null) return Unauthorized();
        var currentUserId = int.Parse(currentUserIdString);

        var request = await _context.Friendships.FindAsync(requestId);
        if (request == null || request.ReceiverId != currentUserId) return NotFound("İstek bulunamadı.");

        if (accept)
        {
            request.Status = "Accepted";
            var u1 = await _context.Users.FindAsync(request.RequesterId);
            var u2 = await _context.Users.FindAsync(request.ReceiverId);
            if (u1 != null) u1.GachaPoints += 50;
            if (u2 != null) u2.GachaPoints += 50;
        }
        else
        {
            _context.Friendships.Remove(request);
        }

        await _context.SaveChangesAsync();
        return Ok(new { Message = accept ? "İstek kabul edildi." : "İstek reddedildi." });
    }

    [HttpGet("friends")]
    public async Task<IActionResult> GetFriends()
    {
        var currentUserIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentUserIdString == null) return Unauthorized();
        var currentUserId = int.Parse(currentUserIdString);

        var friends = await _context.Friendships
            .Where(f => f.Status == "Accepted" && (f.RequesterId == currentUserId || f.ReceiverId == currentUserId))
            .Select(f => f.RequesterId == currentUserId ? f.Receiver : f.Requester)
            .Select(u => new { u.Id, u.Username, u.GachaPoints })
            .ToListAsync();

        return Ok(friends);
    }

    [HttpPost("like/{fortuneId}")]
    public async Task<IActionResult> Like(int fortuneId)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdString == null) return Unauthorized();
        var userId = int.Parse(userIdString);

        if (await _context.Likes.AnyAsync(l => l.DailyFortuneId == fortuneId && l.UserId == userId))
        {
            return BadRequest("Zaten beğendin.");
        }

        _context.Likes.Add(new Like { DailyFortuneId = fortuneId, UserId = userId });

        var currentLiker = await _context.Users.FindAsync(userId);
        var fortune = await _context.DailyFortunes.Include(f => f.User).FirstOrDefaultAsync(f => f.Id == fortuneId);

        if (currentLiker != null) currentLiker.GachaPoints += 1;
        if (fortune?.User != null) fortune.User.GachaPoints += 5;

        await _context.SaveChangesAsync();

        if (fortune?.User != null && fortune.UserId != userId)
        {
            await _hubContext.Clients.User(fortune.UserId.ToString()).SendAsync("ReceiveNotification", new 
            { 
                title = "Falın Beğenildi!", 
                message = $"{currentLiker?.Username} senin bir falını beğendi." 
            });

            if (!string.IsNullOrEmpty(fortune.User.PushToken))
            {
                await _notificationService.SendPushNotificationAsync(
                    fortune.User.PushToken, 
                    "Falın Beğenildi!", 
                    $"{currentLiker?.Username} senin bir falını beğendi."
                );
            }
        }

        return Ok(new { Message = "Beğenildi." });
    }

    [HttpGet("leaderboard")]
    public async Task<IActionResult> GetLeaderboard()
    {
        var leaderlist = await _context.Users
            .OrderByDescending(u => u.GachaPoints)
            .Take(100)
            .ToListAsync();
            
        var result = leaderlist.Select((u, i) => new { u.Username, u.GachaPoints, Rank = i + 1 });

        return Ok(result);
    }
}
