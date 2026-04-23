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
[Route("api/social")]
[Authorize]
public class SocialHubController : ControllerBase
{
    private readonly GachaDbContext _context;
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly INotificationService _notificationService;
    private readonly IQuestService _questService;

    public SocialHubController(GachaDbContext context, IHubContext<NotificationHub> hubContext, INotificationService notificationService, IQuestService questService)
    {
        _context = context;
        _hubContext = hubContext;
        _notificationService = notificationService;
        _questService = questService;
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string q)
    {
        var uidClaim = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (uidClaim == null) return Unauthorized();
        var currentUserId = int.Parse(uidClaim);

        var profiles = await _context.Users
            .Where(u => u.Id != currentUserId && u.Username.Contains(q))
            .Select(u => new { u.Id, u.Username })
            .Take(10)
            .ToListAsync();

        return Ok(profiles);
    }

    [HttpPost("request/{userId}")]
    public async Task<IActionResult> SendRequest(int userId)
    {
        var uidClaim = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (uidClaim == null) return Unauthorized();
        var currentUserId = int.Parse(uidClaim);

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

        var sender = await _context.Users.FindAsync(currentUserId);
        var receiver = await _context.Users.FindAsync(userId);

        await _hubContext.Clients.User(userId.ToString()).SendAsync("ReceiveNotification", new 
        { 
            title = "Yeni Arkadaşlık İsteği", 
            message = $"{sender?.Username} sana bir arkadaşlık isteği gönderdi!" 
        });

        if (receiver != null && !string.IsNullOrEmpty(receiver.PushToken))
        {
            await _notificationService.SendPushNotificationAsync(
                receiver.PushToken, 
                "Yeni Arkadaşlık İsteği", 
                $"{sender?.Username} sana bir arkadaşlık isteği gönderdi!"
            );
        }

        return Ok(new { Message = "Arkadaşlık isteği gönderildi." });
    }

    [HttpPost("respond/{requestId}")]
    public async Task<IActionResult> RespondRequest(int requestId, [FromQuery] bool accept)
    {
        var uidClaim = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (uidClaim == null) return Unauthorized();
        var currentUserId = int.Parse(uidClaim);

        var request = await _context.Friendships.FindAsync(requestId);
        if (request == null || request.ReceiverId != currentUserId) return NotFound("İstek bulunamadı.");

        if (accept)
        {
            request.Status = "Accepted";
            var r1 = await _context.Users.FindAsync(request.RequesterId);
            var r2 = await _context.Users.FindAsync(request.ReceiverId);
            if (r1 != null) r1.GachaPoints += 50;
            if (r2 != null) r2.GachaPoints += 50;
        }
        else
        {
            _context.Friendships.Remove(request);
        }

        await _context.SaveChangesAsync();

        if (accept)
        {
            await _questService.UpdateProgressAsync(request.RequesterId, "Friend");
            await _questService.UpdateProgressAsync(request.ReceiverId, "Friend");
        }

        return Ok(new { Message = accept ? "İstek kabul edildi." : "İstek reddedildi." });
    }

    [HttpGet("friends")]
    public async Task<IActionResult> GetFriends()
    {
        var uidClaim = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (uidClaim == null) return Unauthorized();
        var currentUserId = int.Parse(uidClaim);

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
        var uidClaim = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (uidClaim == null) return Unauthorized();
        var userId = int.Parse(uidClaim);

        if (await _context.Likes.AnyAsync(l => l.DailyFortuneId == fortuneId && l.UserId == userId))
        {
            return BadRequest("Zaten beğendin.");
        }

        _context.Likes.Add(new Like { DailyFortuneId = fortuneId, UserId = userId });

        var currentLiker = await _context.Users.FindAsync(userId);
        var fortune = await _context.DailyFortunes.Include(f => f.Profile).FirstOrDefaultAsync(f => f.Id == fortuneId);

        if (currentLiker != null) currentLiker.GachaPoints += 1;
        if (fortune?.Profile != null) fortune.Profile.GachaPoints += 5;

        await _context.SaveChangesAsync();

        await _questService.UpdateProgressAsync(userId, "Like");

        if (fortune?.Profile != null && fortune.UserId != userId)
        {
            await _hubContext.Clients.User(fortune.UserId.ToString()).SendAsync("ReceiveNotification", new 
            { 
                title = "Falın Beğenildi!", 
                message = $"{currentLiker?.Username} senin bir falını beğendi." 
            });

            if (!string.IsNullOrEmpty(fortune.Profile.PushToken))
            {
                await _notificationService.SendPushNotificationAsync(
                    fortune.Profile.PushToken, 
                    "Falın Beğenildi!", 
                    $"{currentLiker?.Username} senin bir falını beğendi."
                );
            }
        }

        return Ok(new { Message = "Beğenildi." });
    }

    [HttpPost("gift/gp/{friendId}")]
    public async Task<IActionResult> GiftGp(int friendId, [FromQuery] int amount)
    {
        var uidClaim = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (uidClaim == null) return Unauthorized();
        var currentUserId = int.Parse(uidClaim);

        if (amount <= 0) return BadRequest("Geçersiz miktar.");

        var sender = await _context.Users.FindAsync(currentUserId);
        var receiver = await _context.Users.FindAsync(friendId);

        if (sender == null || receiver == null) return NotFound();
        if (sender.GachaPoints < amount) return BadRequest("Yetersiz Gacha Puanı.");

        // Arkadaşlık kontrolü
        var isFriend = await _context.Friendships.AnyAsync(f => 
            f.Status == "Accepted" && 
            ((f.RequesterId == currentUserId && f.ReceiverId == friendId) || 
             (f.RequesterId == friendId && f.ReceiverId == currentUserId)));

        if (!isFriend) return BadRequest("Sadece arkadaşlarına hediye gönderebilirsin.");

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            sender.GachaPoints -= amount;
            receiver.GachaPoints += amount;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            await _hubContext.Clients.User(friendId.ToString()).SendAsync("ReceiveNotification", new 
            { 
                title = "Hediye Geldi! 🎁", 
                message = $"{sender.Username} sana {amount} GP hediye etti!" 
            });

            return Ok(new { Message = "Hediye başarıyla gönderildi.", NewBalance = sender.GachaPoints });
        }
        catch
        {
            await transaction.RollbackAsync();
            return StatusCode(500, "İşlem başarısız.");
        }
    }

    [HttpGet("leaderboard")]
    public async Task<IActionResult> GetLeaderboard()
    {
        var leaderlist = await _context.Users
            .Include(u => u.UserDecorations)
                .ThenInclude(ud => ud.Decoration)
            .OrderByDescending(u => u.GachaPoints)
            .Take(100)
            .ToListAsync();
            
        var leaderboard = leaderlist.Select((u, i) => new 
        { 
            u.Username, 
            u.GachaPoints, 
            Rank = i + 1,
            Equipped = u.UserDecorations
                .Where(ud => ud.IsEquipped)
                .Select(ud => new { ud.Decoration.Type, ud.Decoration.ImageUrl })
                .ToList()
        });

        return Ok(leaderboard);
    }
}
