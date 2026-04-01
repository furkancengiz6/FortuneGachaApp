namespace FortuneGacha.Api.Services;

public interface IImageStorageService
{
    /// <summary>
    /// URL'deki bir resmi indirir ve yerel klasöre kaydeder.
    /// </summary>
    /// <param name="imageUrl">Resmin uzak URL'si</param>
    /// <param name="fileName">Özel dosya adı (opsiyonel)</param>
    /// <returns>Yerel erişim yolu (örn: /uploads/fortunes/xyz.webp)</returns>
    Task<string> SaveImageFromUrlAsync(string imageUrl, string? fileName = null);

    /// <summary>
    /// Statik klasördeki mevcut bir resmin yolunu döner (Mock/Common durumları için).
    /// </summary>
    Task<string> GetStaticImagePathAsync(string rarity);
}
