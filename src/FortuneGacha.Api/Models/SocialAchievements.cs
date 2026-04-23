using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FortuneGacha.Api.Models;

public class Achievement
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public int GpReward { get; set; }

    public string IconKey { get; set; } = "default";
}

public class UserAchievement
{
    public int Id { get; set; }

    public int UserId { get; set; }
    
    [ForeignKey(nameof(UserId))]
    public GachaProfile Profile { get; set; } = null!;

    public int AchievementId { get; set; }
    
    [ForeignKey(nameof(AchievementId))]
    public Achievement Achievement { get; set; } = null!;

    public DateTime EarnedDate { get; set; } = DateTime.UtcNow;
}
