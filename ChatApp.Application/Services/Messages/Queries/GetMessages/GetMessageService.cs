using ChatApp.Application.Interfaces.Contexts;
using ChatApp.Common.Dto;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Application.Services.Messages.Queries.GetMessage;

public class GetMessageService : IGetMessagesService
{
    private readonly IDatabaseContext _context;
    private readonly IUserContext _userContext;
    private readonly IValidator<GetConversationMessagesRequestDto> _validator;
    public GetMessageService(IDatabaseContext context, IValidator<GetConversationMessagesRequestDto> validator,
        IUserContext userContext)
    { _context = context; _validator = validator; _userContext = userContext; }

    public ResultDto<List<GetConversationMessagesResultDto>> Execute(GetConversationMessagesRequestDto request)
    {
        var validation = _validator.Validate(request);
        if (!validation.IsValid) return new() { IsSuccess = false, Message = validation.ToString() };
        var userId = _userContext.UserId;
        if (!_context.Conversations.Any(c => c.Id == request.ConversationId) ||
            !_context.ConversationParticipants.Any(p => p.ConversationId == request.ConversationId &&
                p.UserId == userId && p.LeftDate == default(DateTime)))
            return new() { IsSuccess = false, Message = "Conversation unavailable or access denied." };

        var query = _context.Messages.AsNoTracking().Where(m => m.ConversationId == request.ConversationId);
        if (request.BeforeSequence.HasValue)
            query = query.Where(m => m.SequenceNumber < request.BeforeSequence.Value);
        if (request.AfterSequence.HasValue)
            query = query.Where(m => m.SequenceNumber > request.AfterSequence.Value);
        var ordered = request.AfterSequence.HasValue
            ? query.OrderBy(m => m.SequenceNumber) : query.OrderByDescending(m => m.SequenceNumber);
        var messages = ordered.Take(request.Take).ToList().OrderBy(m => m.SequenceNumber)
            .Select(MessageDto.FromEntity<GetConversationMessagesResultDto>).ToList();
        return new() { IsSuccess = true, Data = messages, Message = "Messages retrieved" };
    }
}
