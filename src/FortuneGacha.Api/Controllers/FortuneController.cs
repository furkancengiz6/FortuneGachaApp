using System.Security.Claims;
using FortuneGacha.Api.Data;
using FortuneGacha.Api.Models;
using FortuneGacha.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using FortuneGacha.Api.Hubs;

namespace FortuneGacha.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FortuneController : ControllerBase
{
    private readonly GachaDbContext _context;
    private readonly IGachaService _gachaService;
    private readonly IHubContext<NotificationHub> _hubContext;

    public FortuneController(GachaDbContext context, IGachaService gachaService, IHubContext<NotificationHub> hubContext)
    {
        _context = context;
        _gachaService = gachaService;
        _hubContext = hubContext;
    }

    [HttpPost("draw")]
    public async Task<IActionResult> Draw([FromQuery] bool boost = false)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdString == null) return Unauthorized();
        var userId = int.Parse(userIdString);

        var user = await _context.Users.FindAsync(userId);
        if (user == null) return Unauthorized();

        if (user.LastDrawDate.HasValue && user.LastDrawDate.Value.Date == DateTime.UtcNow.Date)
        {
            return BadRequest("Günde sadece bir kez fal çekebilirsin. Yarın tekrar gel!");
        }

        if (boost)
        {
            if (user.GachaPoints < 100) return BadRequest("Luck Boost için yetersiz Gacha Puanı (100 GP gerekir).");
            user.GachaPoints -= 100;
        }

        var result = await _gachaService.GenerateFortuneAsync(boost);

        var dailyFortune = new DailyFortune
        {
            UserId = userId,
            FortuneText = result.FortuneText,
            ImageUrl = result.ImageUrl,
            Rarity = result.Rarity,
            DrawDate = DateTime.UtcNow
        };

        user.LastDrawDate = DateTime.UtcNow;
        _context.DailyFortunes.Add(dailyFortune);
        
        await CheckAchievementsAsync(user, result.Rarity);
        await _context.SaveChangesAsync();

        return Ok(new 
        {
            dailyFortune.Id,
            dailyFortune.FortuneText,
            dailyFortune.ImageUrl,
            dailyFortune.Rarity,
            user.GachaPoints
        });
    }

    private async Task CheckAchievementsAsync(User user, string lastRarity)
    {
        if (!await _context.UserAchievements.AnyAsync(ua => ua.UserId == user.Id && ua.AchievementId == 1))
        {
            await GrantAchievement(user, 1);
        }

        if (lastRarity == "Legendary" && !await _context.UserAchievements.AnyAsync(ua => ua.UserId == user.Id && ua.AchievementId == 2))
        {
            await GrantAchievement(user, 2);
        }

        var fortuneCount = await _context.DailyFortunes.CountAsync(f => f.UserId == user.Id);
        if (fortuneCount >= 10 && !await _context.UserAchievements.AnyAsync(ua => ua.UserId == user.Id && ua.AchievementId == 4))
        {
            await GrantAchievement(user, 4);
        }
    }

    private async Task GrantAchievement(User user, int achievementId)
    {
        var achievement = await _context.Achievements.FindAsync(achievementId);
        if (achievement == null) return;

        _context.UserAchievements.Add(new UserAchievement
        {
            UserId = user.Id,
            AchievementId = achievementId,
            EarnedDate = DateTime.UtcNow
        });

        user.GachaPoints += achievement.GpReward;

        await _hubContext.Clients.User(user.Id.ToString()).SendAsync("ReceiveNotification", new 
        { 
            title = "Yeni Başarım!", 
            message = $"'{achievement.Name}' madalyasını kazandın! +{achievement.GpReward} GP" 
        });
    }

    [HttpGet("showcase/{username}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetShowcase(string username)
    {
        var user = await _context.Users
            .Include(u => u.DailyFortunes.OrderByDescending(f => f.DrawDate))
            .ThenInclude(f => f.Likes)
            .FirstOrDefaultAsync(u => u.Username == username);

        if (user == null) return NotFound("User not found.");

        var showcase = user.DailyFortunes
            .Where(f => f.IsPublic)
            .Select(f => new
            {
                f.Id,
                f.FortuneText,
                f.ImageUrl,
                f.Rarity,
                f.DrawDate,
                LikeCount = f.Likes.Count
            });

        return Ok(showcase);
    }

    [HttpGet("my-fortunes")]
    public async Task<IActionResult> GetMyFortunes()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdString == null) return Unauthorized();
        var userId = int.Parse(userIdString);

        var fortunes = await _context.DailyFortunes
            .Where(f => f.UserId == userId)
            .OrderByDescending(f => f.DrawDate)
            .ToListAsync();

        return Ok(fortunes);
    }
}
