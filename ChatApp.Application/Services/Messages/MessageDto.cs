using ChatApp.Domain.Entities.Chat;

namespace ChatApp.Application.Services.Messages;

// Shared wire contract for history, persisted send results and live notifications.
public class MessageDto
{
    public long MessageId { get; set; }
    public long ConversationId { get; set; }
    public long SenderId { get; set; }
    public string Content { get; set; } = "";
    public MessageType MessageType { get; set; }
    public bool IsEdited { get; set; }
    public long SequenceNumber { get; set; }
    public Guid? ClientRequestId { get; set; }
    public DateTime SentAt { get; set; }
    // Transitional aliases for the existing web/mobile clients.
    public long UserId { get => SenderId; set => SenderId = value; }
    public DateTime SendDate { get => SentAt; set => SentAt = value; }

    public static T FromEntity<T>(Message message) where T : MessageDto, new() => new()
    {
        MessageId = message.Id, ConversationId = message.ConversationId,
        SenderId = message.SenderId, Content = message.Content,
        MessageType = message.MessageType, IsEdited = message.IsEdited,
        SequenceNumber = message.SequenceNumber, ClientRequestId = message.ClientRequestId,
        SentAt = DateTime.SpecifyKind(message.CreationDate, DateTimeKind.Utc)
    };
}
