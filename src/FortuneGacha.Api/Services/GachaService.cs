using FortuneGacha.Api.Models;
using System.Collections.Generic;

namespace FortuneGacha.Api.Services;

public record GachaResult(string FortuneText, string ImageUrl, string Rarity, string DailyCommentary);
public record CoffeeReadingResult(string ReadingText, string CardImageUrl, string Rarity, string Summary);

public interface IGachaService
{
    Task<GachaResult> GenerateFortuneAsync(GachaProfile profile, bool isBoosted);
    Task<string> GenerateWeeklyAnalysisAsync(GachaProfile profile, List<DailyFortune> recentFortunes);
    Task<CoffeeReadingResult> ReadCoffeeFortuneAsync(GachaProfile profile, byte[] imageData);
}

public class MockGachaService : IGachaService
{
    private static readonly string[] Fortunes = new[]
    {
        "Bugün şansın parlıyor, yeni bir kapı açılacak.",
        "Beklemediğin bir yerden sürpriz bir haber alacaksın.",
        "Sabırlı ol, meyvesini tatlı bir şekilde yiyeceksin.",
        "İç sesini dinle, o sana doğru yolu gösterecek.",
        "Yaratıcılığın zirvede, bugün bir şeyler üretmelisin.",
        "Geçmişin yüklerinden kurtulma vakti geldi.",
        "Cesur bir adım at, evren seni destekliyor.",
        "Gülümsemen bugün en güçlü silahın olacak."
    };

    private static readonly string[] ImageCategories = new[] { "nebula", "forest", "ocean", "abstract", "aurora", "cityscape" };

    public async Task<GachaResult> GenerateFortuneAsync(GachaProfile profile, bool isBoosted)
    {
        var random = new Random();
        var randVal = random.Next(1, 101);
        string rarity = "Common";

        // Mock Rarity Logic
        if (isBoosted)
        {
            if (randVal <= 20) rarity = "Legendary";
            else if (randVal <= 60) rarity = "Rare";
        }
        else
        {
            if (randVal <= 5) rarity = "Legendary";
            else if (randVal <= 20) rarity = "Rare";
        }

        var text = Fortunes[random.Next(Fortunes.Length)];
        var imageUrl = $"https://picsum.photos/seed/{Guid.NewGuid()}/800/1200";

        string commentary = rarity switch {
            "Legendary" => "Efsanevi bir enerjin var! Bugün imkansız görünen şeyler gerçek olabilir.",
            "Rare" => "Yıldızlar senin için özel bir hizada duruyor. Şanslı bir gün!",
            _ => "Sıradan ama huzurlu bir gün. Dengeyi korumalısın."
        };

        return new GachaResult(text, imageUrl, rarity, commentary);
    }

    public async Task<string> GenerateWeeklyAnalysisAsync(GachaProfile profile, List<DailyFortune> recentFortunes)
    {
        return "Mistik bir haftayı geride bıraktın. Yıldızlar önündeki engellerin kalkacağını söylüyor.";
    }

    private static readonly string[] CoffeeReadings = new[]
    {
        "Fincanında bir yol görünüyor, yakında uzun bir yolculuğa çıkacaksın. Yolun açık olacak, endişelenme.",
        "Fincanının dibinde bir kalp şekli belirdi. Aşk hayatında güzel gelişmeler seni bekliyor.",
        "Kuş figürü görüyorum fincanında. Yakında çok güzel haberler alacaksın, belki de beklediğin o haber.",
        "Fincanında dağ şekli var. Önünde büyük bir engel gibi görünse de, aslında bu bir fırsat kapısı.",
        "Ağaç figürü belirmiş fincanında. Ailevi bağların güçlenecek, köklerin sağlam.",
        "Fincanında yıldız deseni görüyorum. Şans kapını çalacak, hazırlıklı ol!",
        "Balık figürü var fincanında. Maddi açıdan bereketli bir dönem seni bekliyor.",
        "Fincanında göz şekli belirmiş. Seni kıskanan biri var ama enerjin onu alt edecek kadar güçlü."
    };

    private static readonly string[] CoffeeSummaries = new[]
    {
        "Yolculuk ve Keşif", "Aşk ve Romantizm", "Müjdeli Haberler",
        "Fırsatlar Kapıda", "Aile Bağları", "Şans Yıldızı",
        "Bereket ve Bolluk", "Koruyucu Enerji"
    };

    public async Task<CoffeeReadingResult> ReadCoffeeFortuneAsync(GachaProfile profile, byte[] imageData)
    {
        var random = new Random();
        var index = random.Next(CoffeeReadings.Length);
        var reading = CoffeeReadings[index];
        var summary = CoffeeSummaries[index];
        var cardImage = $"https://picsum.photos/seed/coffee{Guid.NewGuid()}/800/1200";

        var randVal = random.Next(1, 101);
        string rarity = randVal <= 10 ? "Legendary" : randVal <= 30 ? "Rare" : "Common";

        return new CoffeeReadingResult(reading, cardImage, rarity, summary);
    }
}
