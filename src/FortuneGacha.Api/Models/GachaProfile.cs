using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace FortuneGacha.Api.Models;

public class GachaProfile
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? LastDrawDate { get; set; }

    public int GachaPoints { get; set; } = 100; // Başlangıç puanı 100 (Bir çekim boost'u için yeterli)

    public string? PushToken { get; set; }
    public string? ZodiacSign { get; set; }
    public string? PersonalInterests { get; set; }
    public string? Bio { get; set; }
    public DateTime? BirthDate { get; set; }

    // Relationship
    public ICollection<DailyFortune> MyDailyFortunes { get; set; } = new List<DailyFortune>();
    public ICollection<UserDecoration> UserDecorations { get; set; } = new List<UserDecoration>();
    public ICollection<UserQuest> UserQuests { get; set; } = new List<UserQuest>();
}
