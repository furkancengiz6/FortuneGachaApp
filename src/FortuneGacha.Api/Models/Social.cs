using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FortuneGacha.Api.Models;

public class Friendship
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int RequesterId { get; set; }

    [Required]
    public int ReceiverId { get; set; }

    [ForeignKey(nameof(RequesterId))]
    public GachaProfile Requester { get; set; } = null!;

    [ForeignKey(nameof(ReceiverId))]
    public GachaProfile Receiver { get; set; } = null!;

    [Required]
    public string Status { get; set; } = "Pending"; // Pending, Accepted, Blocked

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class Like
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int DailyFortuneId { get; set; }

    [Required]
    public int UserId { get; set; }

    [ForeignKey(nameof(DailyFortuneId))]
    public DailyFortune DailyFortune { get; set; } = null!;

    [ForeignKey(nameof(UserId))]
    public GachaProfile Profile { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
