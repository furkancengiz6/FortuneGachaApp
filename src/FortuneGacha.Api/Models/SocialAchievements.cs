using System.ComponentModel.DataAnnotations;

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
    public User User { get; set; } = null!;

    public int AchievementId { get; set; }
    public Achievement Achievement { get; set; } = null!;

    public DateTime EarnedDate { get; set; } = DateTime.UtcNow;
}
