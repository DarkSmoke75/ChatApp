using ChatApp.Domain.Entities.Chat;

namespace ChatApp.Application.Services.Messages.Commands.SendMessage
{
    public class RequestSendMessageDto
    {
        public long ConversationId { get; set; }
        public string Content { get; set; }
        public MessageType MessageType { get; set; }
    }
}
