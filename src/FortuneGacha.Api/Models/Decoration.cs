using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FortuneGacha.Api.Models;

public class Decoration
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Type { get; set; } = "AvatarFrame"; // AvatarFrame, ProfileTheme

    [Required]
    public int Price { get; set; }

    [Required]
    public string ImageUrl { get; set; } = string.Empty; // Frame overlay image or theme color/style

    public string? Rarity { get; set; } = "Common";
}

public class UserDecoration
{
    [Required]
    public int UserId { get; set; }
    
    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;

    [Required]
    public int DecorationId { get; set; }

    [ForeignKey(nameof(DecorationId))]
    public Decoration Decoration { get; set; } = null!;

    public bool IsEquipped { get; set; } = false;

    public DateTime PurchaseDate { get; set; } = DateTime.UtcNow;
}
