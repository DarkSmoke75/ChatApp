using ChatApp.Application.Services.MessageNotifications;
using Microsoft.AspNetCore.SignalR;

namespace Endpoint.Api.Hubs
{
    public class MessageNotificationService : IMessageNotificationService
    {
        private readonly IHubContext<ChatHub> _hub;
        public MessageNotificationService(IHubContext<ChatHub> hub)
        {
            _hub = hub;
        }

        public async Task SendMessageToUser(string userId, object message)
        {
            await _hub.Clients.User(userId)
                .SendAsync("ReceiveMessage", message);
            Console.WriteLine($"Sending message to user: {userId}");
        }
    }
}
