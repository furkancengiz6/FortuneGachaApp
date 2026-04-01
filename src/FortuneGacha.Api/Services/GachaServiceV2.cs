using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using OpenAI.Images;
using OpenAI.Chat;

namespace FortuneGacha.Api.Services;

public class GachaServiceV2 : IGachaService
{
    private readonly IConfiguration _config;
    private readonly IImageStorageService _imageStorage;

    public GachaServiceV2(IConfiguration config, IImageStorageService imageStorage)
    {
        _config = config;
        _imageStorage = imageStorage;
    }

    public async Task<GachaResult> GenerateFortuneAsync(bool isBoosted = false)
    {
        string rarity = CalculateRarity(isBoosted);
        
        // 1. Text Generation (GPT-4o)
        string fortuneText = await GeneratePersonalizedTextAsync(rarity);

        // 2. Image Generation / Selection
        string imageUrl;

        if (rarity == "Rare" || rarity == "Legendary")
        {
            // Real AI Image (DALL-E 3)
            imageUrl = await GenerateAIImageAsync(fortuneText, rarity);
        }
        else
        {
            // Static Image Pool
            imageUrl = await _imageStorage.GetStaticImagePathAsync(rarity);
        }

        return new GachaResult(fortuneText, imageUrl, rarity);
    }

    private string CalculateRarity(bool isBoosted)
    {
        var rand = new Random().Next(1, 101);
        
        if (isBoosted)
        {
            if (rand <= 15) return "Legendary"; // %15
            if (rand <= 50) return "Rare";      // %35
            if (rand <= 75) return "Uncommon";  // %25
            return "Common";                   // %25
        }
        else
        {
            if (rand <= 5) return "Legendary";  // %5
            if (rand <= 15) return "Rare";      // %10
            if (rand <= 40) return "Uncommon";  // %25
            return "Common";                   // %60
        }
    }

    private async Task<string> GeneratePersonalizedTextAsync(string rarity)
    {
        var apiKey = _config["OpenAI:ApiKey"];
        if (string.IsNullOrEmpty(apiKey) || apiKey == "YOUR_OPENAI_API_KEY_HERE")
        {
            return "Şansın bugün seninle ama yıldızlar biraz bulutlu (Mock Fortune).";
        }

        var client = new ChatClient(_config["OpenAI:TextModel"], apiKey);
        
        string prompt = $"Sen mistik bir falcısın. Bana {rarity} nadirliğinde, kısa, etkileyici ve ilham verici bir günlük fal metni yaz. Sadece falın kendisini döndür.";
        
        var response = await client.CompleteChatAsync(prompt);
        return response.Value.Content[0].Text;
    }

    private async Task<string> GenerateAIImageAsync(string fortuneText, string rarity)
    {
        var apiKey = _config["OpenAI:ApiKey"];
        if (string.IsNullOrEmpty(apiKey) || apiKey == "YOUR_OPENAI_API_KEY_HERE")
        {
            return "https://picsum.photos/800/1200"; // Fallback
        }

        var client = new ImageClient(_config["OpenAI:ImageModel"], apiKey);
        
        // Nadirliğe göre prompt güçlendirme
        string quality = rarity == "Legendary" ? "ultra-detailed, ethereal, epic, masterpiece, 8k" : "high quality, mystic, beautiful";
        string prompt = $"A vertical mystic tarot card style illustration. Concept: {fortuneText}. Style: {quality}, cinematic lighting, dark background.";

        var options = new ImageGenerationOptions
        {
            Quality = rarity == "Legendary" ? GeneratedImageQuality.High : GeneratedImageQuality.Standard,
            Size = GeneratedImageSize.W1024xH1792, // Mobil dikey format
            Style = GeneratedImageStyle.Vivid
        };

        var imageResponse = await client.GenerateImageAsync(prompt, options);
        var remoteUrl = imageResponse.Value.ImageUri.ToString();

        // Yerel depolama
        return await _imageStorage.SaveImageFromUrlAsync(remoteUrl);
    }
}
