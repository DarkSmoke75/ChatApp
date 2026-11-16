namespace Endpoint.App.Models.Dtos;

public class MessageDto
{
    public long MessageId { get; set; }
    public long ConversationId { get; set; }
    public long SenderId { get; set; }
    public long UserId { get => SenderId; set => SenderId = value; }
    public string Content { get; set; } = "";
    public bool IsEdited { get; set; }
    public long SequenceNumber { get; set; }
    public MessageType MessageType { get; set; }
    public DateTime SentAt { get; set; }
    public Guid? ClientRequestId { get; set; }
    public string DeliveryState { get; set; } = "Sent";
}
public enum MessageType { Text = 0, Image = 1, Video = 2, File = 3 }

public record SendMessageRequest(long ConversationId, string Content, MessageType MessageType, Guid ClientRequestId);
