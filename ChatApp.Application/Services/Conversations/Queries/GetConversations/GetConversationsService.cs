using ChatApp.Application.Interfaces.Contexts;
using ChatApp.Common.Dto;
using FluentValidation;

namespace ChatApp.Application.Services.Conversations.Queries.GetConversations
{
    public class GetConversationsService : IGetConversationsService
    {
        private readonly IDatabaseContext _context;
        private readonly IUserContext _userContext;
        //private readonly IValidator<GetConversationRequestDto> _validator;
        public GetConversationsService(IDatabaseContext context, IUserContext userContext/*, IValidator<GetConversationRequestDto> validator*/)
        {
            _context = context;
            _userContext = userContext;
            //_validator = validator;
        }
        public ResultDto<List<GetConversationResultDto>> Execute(GetConversationRequestDto request)
        {
            var currentUserId = _userContext.UserId;
            //var validationResult = _validator.Validate(request);
            //if(!validationResult.IsValid)
            //{
            //    return new ResultDto<List<GetConversationResultDto>>()
            //    {
            //        Data = null,
            //        IsSuccess = false,
            //        Message = validationResult.ToString()
            //    };
            //}
            //var conversations = _context.Conversations.Where(p => p.Participants.Any(c => c.UserId == currentUserId))
            //    .Select(c => new
            //    {
            //        Conversation = c,
            //        LastMessage = c.Messages
            //        .OrderByDescending(m => m.SequenceNumber)
            //        .FirstOrDefault()
            //    })
            //    .OrderByDescending(p => p.LastMessage.SequenceNumber)
            //    .Take(request.Take)
            //    .Select(p=>new GetConversationResultDto
            //    {
            //        IsGroup=p.Conversation.IsGroup,
            //        ConversationId=p.Conversation.Id,
            //        Title=p.Conversation.Title,
            //        LastMessage = p.LastMessage.Content,
            //        LastMessageSequence = p.LastMessage.SequenceNumber,
            //        LastMessageTime = p.LastMessage.CreationDate,
            //        OtherUserId = p.Conversation.Participants.Where(p => p.UserId != currentUserId).FirstOrDefault().UserId,

            //    })
            //    .ToList();
            var conversations = _context.Conversations
                .Where(c => c.Participants.Any(p => p.UserId == currentUserId))
                .Select(c => new
                {
                    Conversation = c,
                    LastMessage = c.Messages
                        .OrderByDescending(m => m.SequenceNumber)
                        .FirstOrDefault()
                })
                .OrderByDescending(x => x.LastMessage != null ? x.LastMessage.SequenceNumber : 0)
                .Take(request.Take)
                .Select(x => new GetConversationResultDto
                {

                    IsGroup = x.Conversation.IsGroup,
                    ConversationId = x.Conversation.Id,
                    Title = x.Conversation.Title,

                    LastMessage = x.LastMessage != null ? x.LastMessage.Content : null,
                    LastMessageSequence = x.LastMessage != null ? x.LastMessage.SequenceNumber : (long?)null,
                    LastMessageTime = x.LastMessage != null ? x.LastMessage.CreationDate : (DateTime?)null,
                    OtherUserId = x.Conversation.Participants
                        .Where(p => p.UserId != currentUserId)
                        .Select(p => p.UserId)
                        .FirstOrDefault(),
                    OtherUserName = !x.Conversation.IsGroup
                        ? x.Conversation.Participants
                            .Where(p => p.UserId != currentUserId)
                            .Select(p => p.User.Username)
                            .FirstOrDefault()
                        : null

                })
                .ToList();
            return new ResultDto<List<GetConversationResultDto>>()
            {
                Data= conversations,
                IsSuccess=true,
                Message="Conversation Retrieved Successfully"
            };
        }
    }
    //public class GetConversationsValidator : AbstractValidator<GetConversationRequestDto>
    //{
    //    public GetConversationsValidator() 
    //    {
           
    //    }
    //}

}
