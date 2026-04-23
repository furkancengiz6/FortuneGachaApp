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
    private readonly IQuestService _questService;

    public FortuneController(GachaDbContext context, IGachaService gachaService, IHubContext<NotificationHub> hubContext, IQuestService questService)
    {
        _context = context;
        _gachaService = gachaService;
        _hubContext = hubContext;
        _questService = questService;
    }

    [HttpPost("draw")]
    public async Task<IActionResult> Draw(bool boost = false)
    {
        var uid = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(uid)) return Unauthorized();
        
        var userId = int.Parse(uid);
        var activeUser = await _context.Users.FindAsync(userId);
        if (activeUser == null) return NotFound();

        if (activeUser.LastDrawDate.HasValue && activeUser.LastDrawDate.Value.Date == DateTime.Today)
        {
            return BadRequest("Günde sadece bir kez fal çekebilirsin.");
        }

        bool isBirthday = activeUser.BirthDate.HasValue && 
                         activeUser.BirthDate.Value.Month == DateTime.UtcNow.Month && 
                         activeUser.BirthDate.Value.Day == DateTime.UtcNow.Day;

        if (boost && !isBirthday)
        {
            if (activeUser.GachaPoints < 100) return BadRequest("Yetersiz GP.");
            activeUser.GachaPoints -= 100;
        }

        var result = await _gachaService.GenerateFortuneAsync(activeUser, boost || isBirthday);
        
        if (isBirthday) activeUser.GachaPoints += 500;

        var dailyFortune = new DailyFortune
        {
            UserId = userId,
            Profile = activeUser, // Explicitly link to the tracked user object
            FortuneText = result.FortuneText,
            ImageUrl = result.ImageUrl,
            Rarity = result.Rarity,
            DailyCommentary = result.DailyCommentary,
            DrawDate = DateTime.UtcNow
        };

        activeUser.LastDrawDate = DateTime.UtcNow;
        _context.DailyFortunes.Add(dailyFortune);
        
        await CheckAchievementsAsync(activeUser, result.Rarity);
        await _context.SaveChangesAsync();

        await _questService.UpdateProgressAsync(userId, "Draw");

        return Ok(new 
        {
            dailyFortune.Id,
            dailyFortune.FortuneText,
            dailyFortune.ImageUrl,
            dailyFortune.Rarity,
            dailyFortune.DailyCommentary,
            activeUser.GachaPoints
        });
    }

    private async Task CheckAchievementsAsync(GachaProfile profile, string rarity)
    {
        if (!await _context.UserAchievements.AnyAsync(ua => ua.UserId == profile.Id && ua.AchievementId == 1))
        {
            await GrantAchievement(profile, 1);
        }

        if (rarity == "Legendary" && !await _context.UserAchievements.AnyAsync(ua => ua.UserId == profile.Id && ua.AchievementId == 2))
        {
            await GrantAchievement(profile, 2);
        }

        var count = await _context.DailyFortunes.CountAsync(f => f.UserId == profile.Id);
        if (count >= 10 && !await _context.UserAchievements.AnyAsync(ua => ua.UserId == profile.Id && ua.AchievementId == 4))
        {
            await GrantAchievement(profile, 4);
        }
    }

    private async Task GrantAchievement(GachaProfile profile, int achievementId)
    {
        var achievement = await _context.Achievements.FindAsync(achievementId);
        if (achievement == null) return;

        _context.UserAchievements.Add(new UserAchievement
        {
            UserId = profile.Id,
            Profile = profile,
            AchievementId = achievementId,
            Achievement = achievement,
            EarnedDate = DateTime.UtcNow
        });

        profile.GachaPoints += achievement.GpReward;

        await _hubContext.Clients.User(profile.Id.ToString()).SendAsync("ReceiveNotification", new 
        { 
            title = "Yeni Başarım!", 
            message = $"'{achievement.Name}' madalyasını kazandın! +{achievement.GpReward} GP" 
        });
    }

    [HttpGet("showcase/{username}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetShowcase(string username)
    {
        var target = await _context.Users
            .Include(u => u.MyDailyFortunes)
            .ThenInclude(f => f.Likes)
            .FirstOrDefaultAsync(u => u.Username == username);

        if (target == null) return NotFound("User not found.");

        var showcase = target.MyDailyFortunes
            .Where(f => f.IsPublic)
            .OrderByDescending(f => f.DrawDate)
            .Select(f => new { 
                    f.Id, 
                    f.FortuneText, 
                    f.ImageUrl, 
                    f.Rarity, 
                    f.DailyCommentary,
                    f.DrawDate, 
                    Likes = f.Likes.Count 
                });

        return Ok(showcase);
    }

    [HttpGet("my-fortunes")]
    public async Task<IActionResult> GetMyFortunes()
    {
        var uid = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(uid)) return Unauthorized();
        
        var userId = int.Parse(uid);

        var list = await _context.DailyFortunes
            .Where(f => f.UserId == userId)
            .OrderByDescending(f => f.DrawDate)
            .ToListAsync();

        return Ok(list);
    }

    [HttpGet("analysis")]
    public async Task<IActionResult> GetWeeklyAnalysis()
    {
        var uidClaim = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (uidClaim == null) return Unauthorized();
        var userId = int.Parse(uidClaim);

        var activeUser = await _context.Users.FindAsync(userId);
        if (activeUser == null) return NotFound();

        var recentFortunes = await _context.DailyFortunes
            .Where(f => f.UserId == userId)
            .OrderByDescending(f => f.DrawDate)
            .Take(7)
            .ToListAsync();

        if (recentFortunes.Count < 3)
        {
            return BadRequest("Haftalık analiz için en az 3 adet falın olmalı. Makaraları çevirmeye devam et!");
        }

        var analysis = await _gachaService.GenerateWeeklyAnalysisAsync(activeUser, recentFortunes);

        return Ok(new { Analysis = analysis });
    }
}
