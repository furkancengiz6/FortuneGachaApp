using System.ComponentModel.DataAnnotations;

namespace FortuneGacha.Api.Models;

public class User
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

    // Relationship
    public ICollection<DailyFortune> DailyFortunes { get; set; } = new List<DailyFortune>();
}
