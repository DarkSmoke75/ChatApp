namespace ChatApp.Application.Services.Messages.Commands.SendMessage
{
    public class ResultSendMessageDto
    {
        public long MessageId { get; set; }
        public long ConversationId { get; set; }
        public string Content { get; set; }
        public DateTime SendDate { get; set; }
        public long SenderId { get; set; }
    }
}
