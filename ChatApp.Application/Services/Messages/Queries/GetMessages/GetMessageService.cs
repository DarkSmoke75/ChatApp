using ChatApp.Application.Interfaces.Contexts;
using ChatApp.Common.Dto;
using FluentValidation;

namespace ChatApp.Application.Services.Messages.Queries.GetMessage
{
    public class GetMessageService : IGetMessagesService
    {
        private readonly IDatabaseContext _context;
        private readonly IUserContext _userContext;
        private readonly IValidator<GetConversationMessagesRequestDto> _validator;
        public GetMessageService(IDatabaseContext context, IValidator<GetConversationMessagesRequestDto> validator, IUserContext userContext)
        {
            _context = context;
            _validator = validator;
            _userContext = userContext;
        }
        public ResultDto<List<GetConversationMessagesResultDto>> Execute(GetConversationMessagesRequestDto request)
        {
            var currentUserId=_userContext.UserId;
            var validationResult = _validator.Validate(request);
            if (!validationResult.IsValid)
            {
                return new ResultDto<List<GetConversationMessagesResultDto>>
                {
                    Data = null,
                    IsSuccess = false,
                    Message = validationResult.ToString()
                };
            }
            //var conversation = _context.Conversations.Find(conversationId);
            var conversationExists = _context.Conversations
                .Any(c => c.Id == request.ConversationId);
            if (!conversationExists)
            {
                return new ResultDto<List<GetConversationMessagesResultDto>>()
                {
                    Data = null,
                    IsSuccess = false,
                    Message = "Conversation Not Found"
                };
            }
            var conversationQuery = _context.Messages
                .Where(p => p.ConversationId == request.ConversationId).AsQueryable();
            
            var hasAccess = _context.ConversationParticipants
                .Any(p => p.ConversationId == request.ConversationId && p.UserId == currentUserId);
            if (!hasAccess)
            {
                return new ResultDto<List<GetConversationMessagesResultDto>>
                {
                    IsSuccess = false,
                    Message = "Access Denied"
                };
            }
            if (request.BeforeSequence.HasValue)
            {
                conversationQuery = conversationQuery.Where(p => p.SequenceNumber < request.BeforeSequence.Value);
            }
            var conversation = conversationQuery
                .OrderByDescending(p => p.SequenceNumber)
                .Take(request.Take)
                .Select(p => new GetConversationMessagesResultDto
                {
                    Content = p.Content,
                    ConversationId = request.ConversationId,
                    IsEdited = p.IsEdited,
                    MessageType = p.MessageType,
                    SequenceNumber = p.SequenceNumber,
                    UserId = p.SenderId,
                }).ToList()
                .OrderBy(m => m.SequenceNumber)
                .ToList();

            return new ResultDto<List<GetConversationMessagesResultDto>>()
            {
                Data = conversation,
                IsSuccess = true,
                Message = "Messages Retrieved Successfully"
            };

        }
    }
}
