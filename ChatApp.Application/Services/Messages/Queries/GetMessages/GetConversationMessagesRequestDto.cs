namespace ChatApp.Application.Services.Messages.Queries.GetMessage
{
    public class GetConversationMessagesRequestDto
    {
        public long ConversationId { get; set; }
        public long? BeforeSequence { get; set; }
        public long? AfterSequence { get; set; }
        public int Take { get; set; }
    }
}
