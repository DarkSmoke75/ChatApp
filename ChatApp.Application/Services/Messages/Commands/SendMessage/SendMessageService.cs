using ChatApp.Application.Interfaces.Contexts;
using ChatApp.Application.Services.MessageNotifications;
using ChatApp.Common.Dto;
using ChatApp.Domain.Entities.Chat;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Application.Services.Messages.Commands.SendMessage;

public class SendMessageService : ISendMessageService
{
    private readonly IDatabaseContext _context;
    private readonly IUserContext _userContext;
    private readonly IValidator<RequestSendMessageDto> _validator;
    private readonly IMessageNotificationService _notifications;

    public SendMessageService(IDatabaseContext context, IValidator<RequestSendMessageDto> validator,
        IUserContext userContext, IMessageNotificationService notificationService)
    {
        _context = context;
        _validator = validator;
        _userContext = userContext;
        _notifications = notificationService;
    }

    public async Task<ResultDto<ResultSendMessageDto>> Execute(RequestSendMessageDto request)
    {
        var validation = _validator.Validate(request);
        if (!validation.IsValid) return Failure(validation.ToString());
        var senderId = _userContext.UserId;
        ResultSendMessageDto result;
        List<long> receivers;

        await using (var transaction = await _context.Database.BeginTransactionAsync())
        {
            // Locks are database-wide across API instances and released on commit/rollback.
            // Always acquire the request lock first, then the conversation lock.
            await LockAsync($"chat-request:{senderId}:{request.ClientRequestId:D}");
            await LockAsync($"chat-conversation:{request.ConversationId}");

            var conversationExists = await _context.Conversations.AnyAsync(c => c.Id == request.ConversationId);
            receivers = await _context.ConversationParticipants
                .Where(p => p.ConversationId == request.ConversationId && p.LeftDate == default(DateTime))
                .Select(p => p.UserId).Distinct().ToListAsync();
            if (!conversationExists || !receivers.Contains(senderId))
                return Failure("Conversation unavailable or access denied.");

            // Include deleted messages so a retried request can never recreate one.
            var existing = await _context.Messages.IgnoreQueryFilters().AsNoTracking()
                .SingleOrDefaultAsync(m => m.SenderId == senderId && m.ClientRequestId == request.ClientRequestId);
            if (existing != null)
            {
                if (existing.IsDeleted || existing.ConversationId != request.ConversationId ||
                    existing.Content != request.Content || existing.MessageType != request.MessageType)
                    return Failure("This request ID has already been used for a different or deleted message.");
                return Success(MessageDto.FromEntity<ResultSendMessageDto>(existing));
            }

            var sequence = (await _context.Messages.IgnoreQueryFilters()
                .Where(m => m.ConversationId == request.ConversationId)
                .MaxAsync(m => (long?)m.SequenceNumber) ?? 0) + 1;
            var now = DateTime.UtcNow;
            var message = new Message
            {
                ConversationId = request.ConversationId, SenderId = senderId,
                Content = request.Content, MessageType = request.MessageType,
                ClientRequestId = request.ClientRequestId, SequenceNumber = sequence,
                CreationDate = now
            };
            _context.Messages.Add(message);
            _context.MessagesStatuses.AddRange(receivers.Select(id => new MessageStatus
            {
                Message = message, UserId = id, Status = Status.Sent, CreationDate = now
            }));
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            result = MessageDto.FromEntity<ResultSendMessageDto>(message);
        }

        // Persistence is success. Live transport is best effort; catch-up queries recover
        // missed events. Notify the sender too so their other devices stay synchronized.
        foreach (var receiver in receivers)
        {
            try { await _notifications.SendMessageToUser(receiver.ToString(), result); }
            catch (Exception exception)
            {
                Console.Error.WriteLine($"Live notification failed for message {result.MessageId}: {exception.GetType().Name}");
            }
        }
        return Success(result);
    }

    private Task LockAsync(string resource) => _context.Database.ExecuteSqlInterpolatedAsync($@"
        DECLARE @result int;
        EXEC @result = sys.sp_getapplock @Resource={resource}, @LockMode='Exclusive',
            @LockOwner='Transaction', @LockTimeout=10000;
        IF @result < 0 THROW 51000, 'Message lock unavailable; retry with the same request ID.', 1;");

    private static ResultDto<ResultSendMessageDto> Failure(string message) => new()
        { IsSuccess = false, Message = message };
    private static ResultDto<ResultSendMessageDto> Success(ResultSendMessageDto message) => new()
        { IsSuccess = true, Message = "Message saved", Data = message };
}
