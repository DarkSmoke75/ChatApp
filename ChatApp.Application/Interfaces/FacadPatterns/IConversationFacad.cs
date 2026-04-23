using ChatApp.Application.Services.Conversations.Commands.CreateConversation;
using ChatApp.Application.Services.Conversations.Queries.GetConversations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.Application.Interfaces.FacadPatterns
{
    public interface IConversationFacad
    {
        public ICreateConversationService CreateConversationService { get; }
        public IGetConversationsService GetConversationsService { get; }
    }
}
