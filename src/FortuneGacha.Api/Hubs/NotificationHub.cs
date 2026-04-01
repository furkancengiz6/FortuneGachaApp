using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace FortuneGacha.Api.Hubs;

public class NotificationHub : Hub
{
    // Bağlantı sağlandığında kullanıcıyı kendi ID'sine özel bir gruba ekleyebiliriz
    // Ancak SignalR'ın Context.User.NameIdentifier üzerinden SendToUser desteği var.
    
    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
    }

    public async Task SendNotification(string userId, string title, string message)
    {
        await Clients.User(userId).SendAsync("ReceiveNotification", new { title, message });
    }
}
