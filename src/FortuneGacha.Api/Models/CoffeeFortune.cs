using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FortuneGacha.Api.Models;

public class CoffeeFortune
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public GachaProfile Profile { get; set; } = null!;

    /// <summary>
    /// URL of the uploaded coffee cup photo
    /// </summary>
    public string CupImageUrl { get; set; } = string.Empty;

    /// <summary>
    /// AI-generated fortune reading text
    /// </summary>
    [Required]
    public string ReadingText { get; set; } = string.Empty;

    /// <summary>
    /// AI-generated card image based on the reading
    /// </summary>
    public string CardImageUrl { get; set; } = string.Empty;

    public string Rarity { get; set; } = "Common";

    /// <summary>
    /// Short summary/title of the reading
    /// </summary>
    public string Summary { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
