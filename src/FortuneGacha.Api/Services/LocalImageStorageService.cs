using System;
using System.IO;
using System.Net.Http;
using Microsoft.AspNetCore.Hosting;

namespace FortuneGacha.Api.Services;

public class LocalImageStorageService : IImageStorageService
{
    private readonly IWebHostEnvironment _env;
    private readonly HttpClient _httpClient;
    private const string UploadFolder = "uploads/fortunes";

    public LocalImageStorageService(IWebHostEnvironment env, HttpClient httpClient)
    {
        _env = env;
        _httpClient = httpClient;
    }

    public async Task<string> SaveImageFromUrlAsync(string imageUrl, string? fileName = null)
    {
        // Klasör kontrolü
        string rootPath = Path.Combine(_env.WebRootPath, UploadFolder);
        if (!Directory.Exists(rootPath)) Directory.CreateDirectory(rootPath);

        // Dosya adı belirleme
        string extension = ".webp"; // Genelde OpenAI webp döner ama dinamik de bakılabilir.
        string uniqueFileName = fileName ?? $"{Guid.NewGuid()}{extension}";
        string filePath = Path.Combine(rootPath, uniqueFileName);

        // Resim indirme
        var response = await _httpClient.GetAsync(imageUrl);
        response.EnsureSuccessStatusCode();
        var bytes = await response.Content.ReadAsByteArrayAsync();

        // Yerel dosyaya yazma
        await File.WriteAllBytesAsync(filePath, bytes);

        // Erişilebilir yol (/uploads/fortunes/xyz.webp)
        return $"/{UploadFolder}/{uniqueFileName}";
    }

    public async Task<string> GetStaticImagePathAsync(string rarity)
    {
        // Common/Uncommon durumları için resim bankası.
        // Şimdilik Picsum veya yerel bir placeholder dönülebilir. 
        // User'ın istediği "static assets" modeli için Resources/ klasöründeki dosyalar kullanılabilir.
        
        string[] commonPool = { "c1.jpg", "c2.jpg", "c3.jpg" };
        string[] uncommonPool = { "u1.jpg", "u2.jpg" };

        var rand = new Random();
        string selected = rarity switch
        {
            "Uncommon" => uncommonPool[rand.Next(uncommonPool.Length)],
            _ => commonPool[rand.Next(commonPool.Length)]
        };

        return $"/assets/fallback/{selected}";
    }
}
