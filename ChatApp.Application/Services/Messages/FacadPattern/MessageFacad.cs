using ChatApp.Application.Interfaces.Contexts;
using ChatApp.Application.Interfaces.FacadPatterns;
using ChatApp.Application.Services.MessageNotifications;
using ChatApp.Application.Services.Messages.Commands.SendMessage;
using ChatApp.Application.Services.Messages.Queries.GetMessage;
using FluentValidation;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.Application.Services.Messages.FacadPattern
{
    public class MessageFacad : IMessageFacad
    {
        private readonly IDatabaseContext _context;
        private readonly IHostingEnvironment _environment;
        private readonly IUserContext _userContext;
        private readonly IValidator<RequestSendMessageDto> _sendMessageValidator;
        private readonly IValidator<GetConversationMessagesRequestDto> _getMessagesValidator;
        private readonly IMessageNotificationService _messageNotificationService;

        public MessageFacad(IDatabaseContext context,
            IHostingEnvironment environment,
            IValidator<RequestSendMessageDto> sendMessageValidator,
            IValidator<GetConversationMessagesRequestDto> getMessagesValidator,
            IUserContext userContext,
            IMessageNotificationService messageNotificationService)
        {   
            _context = context;
            _environment = environment;
            _sendMessageValidator = sendMessageValidator;
            _getMessagesValidator = getMessagesValidator;
            _userContext = userContext;
            _messageNotificationService = messageNotificationService;
        }
        private ISendMessageService _sendMessage;
        public ISendMessageService SendMessageService
        {
            get
            { return _sendMessage = _sendMessage ?? new SendMessageService(_context,_sendMessageValidator, _userContext,_messageNotificationService); }
        }
        private IGetMessagesService _getMessage;
        public IGetMessagesService GetMessageService
        {
            get
            {
                return _getMessage = _getMessage ?? new GetMessageService(_context,_getMessagesValidator,_userContext);
            }
        }

    }
}
