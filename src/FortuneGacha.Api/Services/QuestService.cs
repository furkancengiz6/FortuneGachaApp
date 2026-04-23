using FortuneGacha.Api.Data;
using FortuneGacha.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace FortuneGacha.Api.Services;

public interface IQuestService
{
    Task UpdateProgressAsync(int userId, string questType, int amount = 1);
}

public class QuestService : IQuestService
{
    private readonly GachaDbContext _context;

    public QuestService(GachaDbContext context)
    {
        _context = context;
    }

    public async Task UpdateProgressAsync(int userId, string questType, int amount = 1)
    {
        var activeQuests = await _context.Quests
            .Where(q => q.Type == questType)
            .ToListAsync();

        foreach (var quest in activeQuests)
        {
            var userQuest = await _context.UserQuests.FindAsync(userId, quest.Id);
            if (userQuest == null)
            {
                userQuest = new UserQuest
                {
                    UserId = userId,
                    QuestId = quest.Id,
                    CurrentProgress = 0,
                    IsCompleted = false,
                    RewardClaimed = false
                };
                _context.UserQuests.Add(userQuest);
            }

            if (userQuest.IsCompleted) continue;

            userQuest.CurrentProgress += amount;
            if (userQuest.CurrentProgress >= quest.TargetCount)
            {
                userQuest.CurrentProgress = quest.TargetCount;
                userQuest.IsCompleted = true;
            }
            userQuest.LastUpdated = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }
}
