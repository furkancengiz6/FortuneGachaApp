using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace FortuneGacha.Api.Services;

public interface INotificationService
{
    Task SendPushNotificationAsync(string pushToken, string title, string body, object? data = null);
}

public class ExpoNotificationService : INotificationService
{
    private readonly HttpClient _httpClient;
    private const string ExpoPushUrl = "https://exp.host/--/api/v2/push/send";

    public ExpoNotificationService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task SendPushNotificationAsync(string pushToken, string title, string body, object? data = null)
    {
        if (string.IsNullOrEmpty(pushToken)) return;

        var payload = new
        {
            to = pushToken,
            title = title,
            body = body,
            data = data,
            sound = "default"
        };

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        try
        {
            var response = await _httpClient.PostAsync(ExpoPushUrl, content);
            // In production, you'd log the response/errors here
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending push notification: {ex.Message}");
        }
    }
}
