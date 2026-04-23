using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using OpenAI.Images;
using OpenAI.Chat;
using FortuneGacha.Api.Models;
using System.Collections.Generic;
using System.Linq;

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

    public async Task<GachaResult> GenerateFortuneAsync(GachaProfile profile, bool isBoosted)
    {
        string rarity = CalculateRarity(isBoosted);
        
        // 1. Core Generation (Text + Commentary)
        var (fortuneText, dailyCommentary) = await GeneratePersonalizedContentAsync(profile, rarity);

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

        return new GachaResult(fortuneText, imageUrl, rarity, dailyCommentary);
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

    private async Task<(string Prompt, string Commentary)> GeneratePersonalizedContentAsync(GachaProfile profile, string rarity)
    {
        var apiKey = _config["OpenAI:ApiKey"];
        if (string.IsNullOrEmpty(apiKey) || apiKey == "YOUR_OPENAI_API_KEY_HERE")
        {
            return ("Şanslı bir gün (Mock).", "Yıldızlar bugün senin için parlıyor.");
        }

        var client = new ChatClient(_config["OpenAI:TextModel"], apiKey);
        
        string userContext = $"Kullanıcı: {profile.Username}. ";
        if (!string.IsNullOrEmpty(profile.ZodiacSign)) userContext += $"Burcu: {profile.ZodiacSign}. ";
        if (!string.IsNullOrEmpty(profile.PersonalInterests)) userContext += $"İlgi Alanları: {profile.PersonalInterests}. ";

        string prompt = $"Sen mistik bir falcısın. {userContext} Bana {rarity} nadirliğinde bir sonuç üret. " +
                        "Yanıtın şu formatta OLMALI (başka hiçbir şey ekleme): [GÖRSEL BETİMLEMESİ] | [GÜNLÜK ASTROLOJİK YORUM]. " +
                        "Görsel betimlemesi dikey bir kart tasarımını (AI promptu gibi) anlatmalı, günlük yorum ise kullanıcıya direkt 1-2 cümlelik bir tavsiye içermeli.";
        
        var response = await client.CompleteChatAsync(prompt);
        var rawText = response.Value.Content[0].Text;

        var parts = rawText.Split('|', StringSplitOptions.TrimEntries);
        if (parts.Length >= 2)
        {
            return (parts[0].Replace("[", "").Replace("]", ""), parts[1].Replace("[", "").Replace("]", ""));
        }

        return (rawText, "Zamanın akışında gizli bir mesaj var...");
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

    public async Task<string> GenerateWeeklyAnalysisAsync(GachaProfile profile, List<DailyFortune> recentFortunes)
    {
        var apiKey = _config["OpenAI:ApiKey"];
        if (string.IsNullOrEmpty(apiKey) || apiKey == "YOUR_OPENAI_API_KEY_HERE")
            return "Analiz şu an yapılamıyor. API anahtarı eksik.";

        var client = new ChatClient(_config["OpenAI:TextModel"], apiKey);
        
        string history = string.Join("\n", recentFortunes.Select(f => $"- {f.DailyCommentary}"));
        
        string prompt = $"Sen usta bir astroloji ve kader analistsin. Kullanıcı: {profile.Username}. " +
                        $"Son 1 haftada çekilen falların özetleri şunlar:\n{history}\n\n" +
                        "Bu fallara dayanarak kullanıcının genel haftalık kaderini özetle ve ona özel 2-3 cümlelik mistik bir tavsiye ver. " +
                        "Yanıtın profesyonel, merak uyandırıcı ve samimi olsun.";

        var response = await client.CompleteChatAsync(prompt);
        return response.Value.Content[0].Text;
    }

    public async Task<CoffeeReadingResult> ReadCoffeeFortuneAsync(GachaProfile profile, byte[] imageData)
    {
        var apiKey = _config["OpenAI:ApiKey"];
        if (string.IsNullOrEmpty(apiKey) || apiKey == "YOUR_OPENAI_API_KEY_HERE")
        {
            // Fallback to mock
            return new CoffeeReadingResult(
                "Fincanında güzel şekiller görünüyor (Mock).",
                "https://picsum.photos/800/1200",
                "Common",
                "Kahve Falı Yorumu"
            );
        }

        // 1. GPT-4 Vision: Fincan fotoğrafını analiz et
        var chatClient = new ChatClient(_config["OpenAI:TextModel"], apiKey);

        var base64Image = Convert.ToBase64String(imageData);
        var imageContent = ChatMessageContentPart.CreateImagePart(
            new BinaryData(imageData), "image/jpeg"
        );
        var textContent = ChatMessageContentPart.CreateTextPart(
            "Sen deneyimli bir Türk kahvesi falcısısın. Bu fincan fotoğrafını analiz et. " +
            "Fincandaki şekilleri, desenleri ve figürleri yorumla. " +
            "Yanıtın şu formatta OLMALI (başka hiçbir şey ekleme): " +
            "[KISA_BAŞLIK] | [DETAYLI_YORUM] | [KART_BETİMLEMESİ]. " +
            "KISA_BAŞLIK: 2-3 kelimelik özet (örn: 'Aşk Kapıda'). " +
            "DETAYLI_YORUM: Fincandaki şekillere dayalı 3-5 cümlelik mistik ve samimi yorum. " +
            "KART_BETİMLEMESİ: Yoruma uygun, dikey bir sanat kartı için İngilizce DALL-E promptu."
        );

        var userMessage = new UserChatMessage(textContent, imageContent);
        var chatResponse = await chatClient.CompleteChatAsync(new ChatMessage[] { userMessage });
        var rawText = chatResponse.Value.Content[0].Text;

        // Parse response
        var parts = rawText.Split('|', StringSplitOptions.TrimEntries);
        string summary = parts.Length >= 1 ? parts[0].Replace("[", "").Replace("]", "") : "Kahve Falı";
        string readingText = parts.Length >= 2 ? parts[1].Replace("[", "").Replace("]", "") : rawText;
        string cardPrompt = parts.Length >= 3 ? parts[2].Replace("[", "").Replace("]", "") : "A mystical Turkish coffee cup fortune card";

        // 2. Determine rarity based on reading length/quality
        string rarity = readingText.Length > 200 ? "Legendary" : readingText.Length > 100 ? "Rare" : "Common";

        // 3. DALL-E: Kart görseli oluştur
        string cardImageUrl;
        try
        {
            var imageClient = new ImageClient(_config["OpenAI:ImageModel"], apiKey);
            string quality = rarity == "Legendary" ? "ultra-detailed, ethereal, epic, masterpiece" : "high quality, mystic";
            string fullPrompt = $"A vertical mystic fortune card illustration. {cardPrompt}. Style: {quality}, Turkish coffee fortune art, dark mystical background, golden accents, ornate frame.";

            var options = new ImageGenerationOptions
            {
                Quality = rarity == "Legendary" ? GeneratedImageQuality.High : GeneratedImageQuality.Standard,
                Size = GeneratedImageSize.W1024xH1792,
                Style = GeneratedImageStyle.Vivid
            };

            var imageResponse = await imageClient.GenerateImageAsync(fullPrompt, options);
            cardImageUrl = await _imageStorage.SaveImageFromUrlAsync(imageResponse.Value.ImageUri.ToString());
        }
        catch
        {
            cardImageUrl = $"https://picsum.photos/seed/coffee{Guid.NewGuid()}/800/1200";
        }

        return new CoffeeReadingResult(readingText, cardImageUrl, rarity, summary);
    }
}
