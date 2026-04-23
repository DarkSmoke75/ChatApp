using ChatApp.Domain.Entities.Common;
using ChatApp.Domain.Entities.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.Domain.Entities.Chat
{
    public class MessageStatus:BaseEntity<long>
    {
        public long MessageId { get; set; }
        public virtual Message Message { get; set; }
        public long UserId { get; set; }
        public virtual User User { get; set; }
        public Status Status { get; set; }
    }
    public enum Status
    {
        Sent=0,
        Delivered=1,
        Read=2
    }
}
