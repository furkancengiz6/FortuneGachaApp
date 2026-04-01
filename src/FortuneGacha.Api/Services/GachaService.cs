namespace FortuneGacha.Api.Services;

public record GachaResult(string FortuneText, string ImageUrl, string Rarity);

public interface IGachaService
{
    Task<GachaResult> GenerateFortuneAsync(bool isBoosted = false);
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

    public Task<GachaResult> GenerateFortuneAsync(bool isBoosted = false)
    {
        var random = new Random();
        var text = Fortunes[random.Next(Fortunes.Length)];
        var category = ImageCategories[random.Next(ImageCategories.Length)];
        
        // Mock image from Unsplash/Picsum
        var imageUrl = $"https://picsum.photos/seed/{Guid.NewGuid()}/800/1200";

        return Task.FromResult(new GachaResult(text, imageUrl, "Common"));
    }
}
