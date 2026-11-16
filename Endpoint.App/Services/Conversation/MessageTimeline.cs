using System.Collections.ObjectModel;
using Endpoint.App.Models.Dtos;

namespace Endpoint.App.Services.Conversation;

public sealed class MessageTimeline
{
    public long ConversationId { get; }
    public long RecoveryCursor { get; private set; }
    public ObservableCollection<MessageDto> Messages { get; } = new();
    public MessageTimeline(long conversationId) { ConversationId = conversationId; }

    public void Merge(MessageDto message)
    {
        if (message.ConversationId != ConversationId) return;
        if (message.MessageId > 0 && Messages.Any(m => m.MessageId == message.MessageId)) return;
        var pending = Messages.FirstOrDefault(m => m.MessageId == 0 &&
            m.ClientRequestId.HasValue && m.ClientRequestId == message.ClientRequestId);
        if (pending != null) Messages.Remove(pending);
        var index = 0;
        if (message.MessageId == 0) index = Messages.Count;
        else while (index < Messages.Count && Messages[index].MessageId > 0 &&
            Messages[index].SequenceNumber < message.SequenceNumber) index++;
        Messages.Insert(index, message);
    }

    public void ApplyPage(IEnumerable<MessageDto> messages)
    {
        foreach (var message in messages)
        {
            Merge(message);
            if (message.ConversationId == ConversationId)
                RecoveryCursor = Math.Max(RecoveryCursor, message.SequenceNumber);
        }
    }
}
