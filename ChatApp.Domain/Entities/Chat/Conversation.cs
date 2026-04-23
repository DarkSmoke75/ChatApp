using ChatApp.Domain.Entities.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.Domain.Entities.Chat
{
    public class Conversation:BaseEntity<long>
    {
        public string? Title { get; set; }
        public bool IsGroup { get; set; }
        public virtual ICollection<ConversationParticipant> Participants { get; set; }
        public virtual ICollection<Message> Messages { get; set; }
    }
}
