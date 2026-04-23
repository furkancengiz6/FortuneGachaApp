using System.Security.Claims;
using FortuneGacha.Api.Data;
using FortuneGacha.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FortuneGacha.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class QuestController : ControllerBase
{
    private readonly GachaDbContext _context;

    public QuestController(GachaDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetQuests()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        
        var quests = await _context.Quests.ToListAsync();
        var userQuests = await _context.UserQuests
            .Where(uq => uq.UserId == userId)
            .ToDictionaryAsync(uq => uq.QuestId);

        var result = quests.Select(q => {
            userQuests.TryGetValue(q.Id, out var uQuest);
            return new
            {
                q.Id,
                q.Name,
                q.Description,
                q.TargetCount,
                q.GpReward,
                CurrentProgress = uQuest?.CurrentProgress ?? 0,
                IsCompleted = uQuest?.IsCompleted ?? false,
                RewardClaimed = uQuest?.RewardClaimed ?? false
            };
        });

        return Ok(result);
    }

    [HttpPost("claim/{id}")]
    public async Task<IActionResult> ClaimReward(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await _context.Users.FindAsync(userId);
        var userQuest = await _context.UserQuests.Include(uq => uq.Quest).FirstOrDefaultAsync(uq => uq.UserId == userId && uq.QuestId == id);

        if (user == null || userQuest == null) return NotFound();
        if (!userQuest.IsCompleted) return BadRequest("Görev henüz tamamlanmadı.");
        if (userQuest.RewardClaimed) return BadRequest("Ödül zaten alındı.");

        userQuest.RewardClaimed = true;
        user.GachaPoints += userQuest.Quest.GpReward;

        await _context.SaveChangesAsync();

        return Ok(new { Message = "Ödül başarıyla alındı!", NewBalance = user.GachaPoints });
    }
}
