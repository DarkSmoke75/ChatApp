namespace Endpoint.Site.Models.ViewModels.ConversationViewModel
{
    public class ChatViewModel
    {
        public long ConversationId { get; set; }
        public List<MessageViewModel> Messages { get; set; } = new();
    }
}
