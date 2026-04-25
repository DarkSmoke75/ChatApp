using ChatApp.Application.Services.Conversations.Queries.GetConversations;

namespace Endpoint.Site.Models.ViewModels.ConversationViewModel
{
    public class ConversationViewModel
    {
        public long Id { get; set; }
        public string? Title { get; set; }
        public string? OtherUserName { get; set; }
        public bool IsGroup { get; set; }
        public string LastMessage { get; set; }
    }
}
