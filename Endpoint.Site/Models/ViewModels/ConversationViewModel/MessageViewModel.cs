using ChatApp.Domain.Entities.Chat;

namespace Endpoint.Site.Models.ViewModels.ConversationViewModel
{
    public class MessageViewModel
    {
        public long ConversationId { get; set; }
        public long UserId { get; set; }
        public string Content { get; set; }
        public bool IsEdited { get; set; }
        public MessageType MessageType { get; set; }
        public long SequenceNumber { get; set; }
        public bool IsMine { get; set; }
    }
}
