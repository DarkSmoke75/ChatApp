using ChatApp.Domain.Entities.Common;
using ChatApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChatApp.Domain.Entities.Users;

namespace ChatApp.Domain.Entities.Chat
{
    public class ConversationParticipant:BaseEntity<long>
    {
        public long ConversationId { get; set; }
        public virtual Conversation Conversation { get; set; }
        public long UserId { get; set; }
        public virtual User User { get; set; }
        public DateTime JoinDate { get; set; }
        public DateTime LeftDate { get; set; }
        public ParticipantRole ParticipantRole { get; set; }
    }
    public enum ParticipantRole
    {
        Admin=0,
        Member=1
    }
}
