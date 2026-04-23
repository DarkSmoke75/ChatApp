using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Endpoint.Api.Hubs
{
    public class ChatHub:Hub
    {
        private static readonly Dictionary<string, string> OnlineUsers = new();

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Console.WriteLine("UserIdentifier: " + Context.UserIdentifier);
            Console.WriteLine("UserId: " + userId);

            if (!string.IsNullOrEmpty(userId))
            {
                OnlineUsers[userId] = Context.ConnectionId;
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = OnlineUsers.FirstOrDefault(x => x.Value == Context.ConnectionId).Key;

            if (!string.IsNullOrEmpty(userId))
            {
                OnlineUsers.Remove(userId);
            }

            await base.OnDisconnectedAsync(exception);
        }
        public async Task SendMessage(string conversationId, string message, string receiverUserId)
        {
            var senderUserId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            await Clients.User(receiverUserId)
                .SendAsync("ReceiveMessage", new
                {
                    ConversationId = conversationId,
                    Message = message,
                    SenderId = senderUserId,
                    SentAt = DateTime.UtcNow
                });
        }
    }
}
