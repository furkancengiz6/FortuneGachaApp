using System.Security.Claims;
using FortuneGacha.Api.Data;
using FortuneGacha.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FortuneGacha.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ShopController : ControllerBase
{
    private readonly GachaDbContext _context;

    public ShopController(GachaDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetItems()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        
        var decorations = await _context.Decorations.ToListAsync();
        var ownedIds = await _context.UserDecorations
            .Where(ud => ud.UserId == userId)
            .Select(ud => ud.DecorationId)
            .ToListAsync();

        var result = decorations.Select(d => new
        {
            d.Id,
            d.Name,
            d.Type,
            d.Price,
            d.ImageUrl,
            d.Rarity,
            IsOwned = ownedIds.Contains(d.Id)
        });

        return Ok(result);
    }

    [HttpPost("buy/{id}")]
    public async Task<IActionResult> BuyItem(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await _context.Users.FindAsync(userId);
        var decoration = await _context.Decorations.FindAsync(id);

        if (user == null || decoration == null) return NotFound();

        if (await _context.UserDecorations.AnyAsync(ud => ud.UserId == userId && ud.DecorationId == id))
            return BadRequest("Zaten bu eşyaya sahipsin.");

        if (user.GachaPoints < decoration.Price)
            return BadRequest("Yetersiz Gacha Puanı.");

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            user.GachaPoints -= decoration.Price;
            _context.UserDecorations.Add(new UserDecoration
            {
                UserId = userId,
                DecorationId = id,
                PurchaseDate = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(new { Message = "Satın alma başarılı!", NewBalance = user.GachaPoints });
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, "İşlem sırasında bir hata oluştu.");
        }
    }

    [HttpPost("equip/{id}")]
    public async Task<IActionResult> EquipItem(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        
        var owned = await _context.UserDecorations.FindAsync(userId, id);
        if (owned == null) return BadRequest("Bu eşyaya sahip değilsin.");

        var decoration = await _context.Decorations.FindAsync(id);
        if (decoration == null) return NotFound();

        // Aynı tipten (Frame/Theme) diğerlerini çıkar
        var others = await _context.UserDecorations
            .Include(ud => ud.Decoration)
            .Where(ud => ud.UserId == userId && ud.Decoration.Type == decoration.Type && ud.DecorationId != id)
            .ToListAsync();

        foreach (var item in others) item.IsEquipped = false;
        
        owned.IsEquipped = true;

        await _context.SaveChangesAsync();
        return Ok(new { Message = "Eşya kuşandın!" });
    }
}
