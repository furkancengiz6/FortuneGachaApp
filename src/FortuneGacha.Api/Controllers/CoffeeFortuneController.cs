using System.Security.Claims;
using FortuneGacha.Api.Data;
using FortuneGacha.Api.Models;
using FortuneGacha.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FortuneGacha.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CoffeeController : ControllerBase
{
    private readonly GachaDbContext _context;
    private readonly IGachaService _gachaService;

    public CoffeeController(GachaDbContext context, IGachaService gachaService)
    {
        _context = context;
        _gachaService = gachaService;
    }

    /// <summary>
    /// Türk kahvesi falı oku — fincan fotoğrafı yükle, AI yorumlasın
    /// </summary>
    [HttpPost("read")]
    public async Task<IActionResult> ReadCoffeeFortune(IFormFile cupImage)
    {
        var uid = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(uid)) return Unauthorized();

        var userId = int.Parse(uid);
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return NotFound("Kullanıcı bulunamadı.");

        // Günlük limit kontrolü: 1 ücretsiz + ek okumalar 50 GP
        var todaysReadings = await _context.CoffeeFortunes
            .CountAsync(cf => cf.UserId == userId && cf.CreatedAt.Date == DateTime.UtcNow.Date);

        if (todaysReadings >= 1)
        {
            if (user.GachaPoints < 50)
                return BadRequest("Bugün zaten ücretsiz kahve falın baktırıldı. Ek okuma için 50 GP gerekli.");

            user.GachaPoints -= 50;
        }

        // Fotoğrafı oku
        if (cupImage == null || cupImage.Length == 0)
            return BadRequest("Lütfen fincan fotoğrafı yükleyin.");

        byte[] imageData;
        using (var ms = new MemoryStream())
        {
            await cupImage.CopyToAsync(ms);
            imageData = ms.ToArray();
        }

        // AI Analiz
        var result = await _gachaService.ReadCoffeeFortuneAsync(user, imageData);

        // Fincan fotoğrafını kaydet
        var cupFileName = $"cup_{userId}_{DateTime.UtcNow.Ticks}.jpg";
        var cupPath = Path.Combine("wwwroot", "uploads", "cups");
        Directory.CreateDirectory(cupPath);
        var fullPath = Path.Combine(cupPath, cupFileName);
        await System.IO.File.WriteAllBytesAsync(fullPath, imageData);
        var cupImageUrl = $"/uploads/cups/{cupFileName}";

        // Veritabanına kaydet
        var coffeeFortune = new CoffeeFortune
        {
            UserId = userId,
            Profile = user,
            CupImageUrl = cupImageUrl,
            ReadingText = result.ReadingText,
            CardImageUrl = result.CardImageUrl,
            Rarity = result.Rarity,
            Summary = result.Summary,
            CreatedAt = DateTime.UtcNow
        };

        _context.CoffeeFortunes.Add(coffeeFortune);

        // GP ödülü: Nadirliğe göre
        int gpReward = result.Rarity switch
        {
            "Legendary" => 50,
            "Rare" => 20,
            _ => 5
        };
        user.GachaPoints += gpReward;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            coffeeFortune.Id,
            coffeeFortune.ReadingText,
            coffeeFortune.CardImageUrl,
            coffeeFortune.Rarity,
            coffeeFortune.Summary,
            coffeeFortune.CupImageUrl,
            GpReward = gpReward,
            user.GachaPoints,
            RemainingFreeReads = todaysReadings >= 1 ? 0 : 1
        });
    }

    /// <summary>
    /// Kullanıcının kahve falı geçmişi
    /// </summary>
    [HttpGet("my-readings")]
    public async Task<IActionResult> GetMyReadings()
    {
        var uid = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(uid)) return Unauthorized();

        var userId = int.Parse(uid);
        var readings = await _context.CoffeeFortunes
            .Where(cf => cf.UserId == userId)
            .OrderByDescending(cf => cf.CreatedAt)
            .Select(cf => new
            {
                cf.Id,
                cf.ReadingText,
                cf.CardImageUrl,
                cf.Rarity,
                cf.Summary,
                cf.CupImageUrl,
                cf.CreatedAt
            })
            .ToListAsync();

        return Ok(readings);
    }
}
