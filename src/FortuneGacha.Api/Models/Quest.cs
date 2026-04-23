using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FortuneGacha.Api.Models;

public class Quest
{
    [Key]
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // e.g. "Like", "Draw", "Friend"
    public int TargetCount { get; set; }
    public int GpReward { get; set; }
}

public class UserQuest
{
    public int UserId { get; set; }
    
    [ForeignKey(nameof(UserId))]
    public GachaProfile User { get; set; } = null!;

    public int QuestId { get; set; }
    
    [ForeignKey(nameof(QuestId))]
    public Quest Quest { get; set; } = null!;

    public int CurrentProgress { get; set; }
    public bool IsCompleted { get; set; }
    public bool RewardClaimed { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}
