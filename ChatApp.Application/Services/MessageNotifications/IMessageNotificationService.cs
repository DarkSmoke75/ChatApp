using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.Application.Services.MessageNotifications
{
    public interface IMessageNotificationService
    {
        public Task SendMessageToUser(string userId, object message);
    }
    
}
