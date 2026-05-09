namespace Endpoint.App.Models.Dtos
{
    public class MessageDto
    {
        public long MessageId { get; set; }
        public long ConversationId { get; set; }
        public long UserId { get; set; }
        public string Content { get; set; }
        public bool IsEdited { get; set; }
        public long SequenceNumber { get; set; }
        public MessageType Messagetype { get; set; }
        public DateTime SentAt { get; set; }
    }
    public enum MessageType
    {
        Text = 0,
        Image = 1,
        Video = 2,
        File = 3
    }
}
