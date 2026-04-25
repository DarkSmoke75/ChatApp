namespace Endpoint.Site.Models.ViewModels.ConversationViewModel
{
    public class MessageViewModel
    {
        public long Id { get; set; }
        public long ConversationId { get; set; }
        public string Content { get; set; }
    }
}
