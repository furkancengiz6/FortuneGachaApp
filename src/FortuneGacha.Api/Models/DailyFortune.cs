using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FortuneGacha.Api.Models;

public class DailyFortune
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public GachaProfile Profile { get; set; } = null!;

    [Required]
    public string FortuneText { get; set; } = string.Empty;

    [Required]
    public string ImageUrl { get; set; } = string.Empty;

    public string Rarity { get; set; } = "Common"; // Common, Rare, Legendary
    public string DailyCommentary { get; set; } = string.Empty;

    public bool IsPublic { get; set; } = true;
    public bool IsForSale { get; set; } = false;
    public int? Price { get; set; }

    [Required]
    public DateTime DrawDate { get; set; } = DateTime.UtcNow;

    // Relationship
    public ICollection<Like> Likes { get; set; } = new List<Like>();
}
