using ChatApp.Domain.Entities.Common;
using ChatApp.Domain.Entities.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.Domain.Entities.Chat
{
    public class Message:BaseEntity<long>
    {
        public long ConversationId { get; set; }
        public virtual Conversation Conversation { get; set; }
        public long SenderId { get; set; }
        public virtual User Sender { get; set; }
        public string Content { get; set; }
        public bool IsEdited { get; set; }
        public DateTime? EditDate { get; set; }
        public MessageType MessageType { get; set; }
        public long SequenceNumber { get; set; }
    }
    public enum MessageType
    {
        Text=0,
        Image=1,
        Video=2,
        File=3
    }
}
