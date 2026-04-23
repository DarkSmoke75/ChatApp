using ChatApp.Application.Services.Messages.Commands.SendMessage;
using ChatApp.Application.Services.Messages.Queries.GetMessage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.Application.Interfaces.FacadPatterns
{
    public interface IMessageFacad
    {
        public ISendMessageService SendMessageService { get; }
        public IGetMessagesService GetMessageService { get; }
    }
}
