using System.Security.Claims;
using FortuneGacha.Api.Data;
using FortuneGacha.Api.Models;
using FortuneGacha.Api.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using FortuneGacha.Api.Services;

namespace FortuneGacha.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MarketplaceController : ControllerBase
{
    private readonly GachaDbContext _context;
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly INotificationService _notificationService;

    public MarketplaceController(GachaDbContext context, IHubContext<NotificationHub> hubContext, INotificationService notificationService)
    {
        _context = context;
        _hubContext = hubContext;
        _notificationService = notificationService;
    }

    [HttpGet]
    public async Task<IActionResult> GetItems([FromQuery] string? rarity = null)
    {
        var query = _context.DailyFortunes
            .Include(f => f.User)
            .Where(f => f.IsForSale);

        if (!string.IsNullOrEmpty(rarity))
        {
            query = query.Where(f => f.Rarity == rarity);
        }

        var items = await query
            .OrderByDescending(f => f.DrawDate)
            .Select(f => new
            {
                f.Id,
                f.FortuneText,
                f.ImageUrl,
                f.Rarity,
                f.Price,
                Seller = f.User.Username
            })
            .ToListAsync();

        return Ok(items);
    }

    [HttpPost("list/{id}")]
    public async Task<IActionResult> ListForSale(int id, [FromQuery] int price)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var fortune = await _context.DailyFortunes.FindAsync(id);

        if (fortune == null || fortune.UserId != userId)
            return NotFound("Fal bulunamadı veya sana ait değil.");

        if (price < 10) return BadRequest("Minimum satış fiyatı 10 GP olmalıdır.");

        fortune.IsForSale = true;
        fortune.Price = price;

        await _context.SaveChangesAsync();
        return Ok(new { Message = "Fal pazarda listelendi." });
    }

    [HttpPost("buy/{id}")]
    public async Task<IActionResult> BuyItem(int id)
    {
        var buyerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var fortune = await _context.DailyFortunes
                .Include(f => f.User)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (fortune == null || !fortune.IsForSale || fortune.Price == null)
                return NotFound("Bu fal artık satılık değil.");

            if (fortune.UserId == buyerId)
                return BadRequest("Kendi falını satın alamazsın.");

            var buyer = await _context.Users.FindAsync(buyerId);
            var seller = fortune.User;

            if (buyer == null || buyer.GachaPoints < fortune.Price.Value)
                return BadRequest("Yetersiz Gacha Puanı.");

            // Ekonomi Transferi
            buyer.GachaPoints -= fortune.Price.Value;
            seller.GachaPoints += fortune.Price.Value;

            // Sahiplik Değişimi
            fortune.UserId = buyerId;
            fortune.IsForSale = false;
            fortune.Price = null;
            fortune.IsPublic = true; // Satın alınan fal vitrine eklenir

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            // SignalR Bildirimleri
            // 1. Satıcıya bildirim
            await _hubContext.Clients.User(seller.Id.ToString()).SendAsync("ReceiveNotification", new 
            { 
                title = "Falın Satıldı! 💰", 
                message = $"{buyer.Username} senin bir falını satın aldı. +{fortune.Price.Value} GP kazandın!" 
            });

            // Push: Bildirim gönder (Satıcıya)
            if (!string.IsNullOrEmpty(seller.PushToken))
            {
                await _notificationService.SendPushNotificationAsync(
                    seller.PushToken, 
                    "Falın Satıldı! 💰", 
                    $"{buyer.Username} senin bir falını satın aldı. +{fortune.Price.Value} GP kazandın!"
                );
            }

            return Ok(new { Message = "Satın alma başarılı!", NewBalance = buyer.GachaPoints });
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, "İşlem sırasında bir hata oluştu.");
        }
    }
}
