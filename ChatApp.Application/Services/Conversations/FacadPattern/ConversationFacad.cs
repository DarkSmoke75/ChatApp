using ChatApp.Application.Interfaces.Contexts;
using ChatApp.Application.Interfaces.FacadPatterns;
using ChatApp.Application.Services.Conversations.Commands.CreateConversation;
using ChatApp.Application.Services.Conversations.Queries.GetConversations;
using FluentValidation;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.Application.Services.Conversations.FacadPattern
{
    public class ConversationFacad : IConversationFacad
    {
        private readonly IDatabaseContext _context;
        private readonly IUserContext _userContext;
        private readonly IHostingEnvironment _environment;
        private readonly IValidator<CreateConversationDto> _validator;

        public ConversationFacad(IDatabaseContext context, IHostingEnvironment environment, IValidator<CreateConversationDto> validator,IUserContext userContext)
        {
            _context = context;
            _environment = environment;
            _validator = validator;
            _userContext = userContext;
        }
        private ICreateConversationService _createConversation;
        public ICreateConversationService CreateConversationService
        {
            get
            {
                return _createConversation = _createConversation ?? new CreateConversationService(_context,_validator,_userContext);
            }
        }
        private IGetConversationsService _getConversations;
        public IGetConversationsService GetConversationsService
        {
            get
            {
                return _getConversations = _getConversations ?? new GetConversationsService(_context, _userContext);
            }
        }


    }
}
