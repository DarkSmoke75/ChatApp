using ChatApp.Domain.Entities.Chat;

namespace ChatApp.Application.Services.Messages.Queries.GetMessage
{
    public class GetConversationMessagesResultDto
    {
        public long ConversationId { get; set; }
        public long UserId { get; set; }
        public string Content { get; set; }
        public bool IsEdited { get; set; }
        public MessageType MessageType { get; set; }
        public long SequenceNumber { get; set; }
    }
}
