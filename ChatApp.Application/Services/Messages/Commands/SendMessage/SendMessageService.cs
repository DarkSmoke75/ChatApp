using ChatApp.Application.Interfaces.Contexts;
using ChatApp.Application.Services.MessageNotifications;
using ChatApp.Common.Dto;
using ChatApp.Domain.Entities.Chat;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Application.Services.Messages.Commands.SendMessage
{
    public class SendMessageService : ISendMessageService
    {
        private readonly IDatabaseContext _context;
        private readonly IUserContext _userContext;
        private readonly IValidator<RequestSendMessageDto> _validator;
        private readonly IMessageNotificationService _notificationService;
        public SendMessageService(IDatabaseContext context, IValidator<RequestSendMessageDto> validator, IUserContext userContext, IMessageNotificationService notificationService)
        {
            _context = context;
            _validator = validator;
            _userContext = userContext;
            _notificationService = notificationService;
        }
        public async Task<ResultDto<ResultSendMessageDto>> Execute(RequestSendMessageDto request)
        {
            Console.WriteLine("SEND MESSAGE HIT");
            var validationResult = _validator.Validate(request);
            if (validationResult.IsValid)
            {
                var conversation = await _context.Conversations.FirstOrDefaultAsync(p=>p.Id==request.ConversationId);
                if (conversation == null)
                {
                    return new ResultDto<ResultSendMessageDto>()
                    {
                        Data=null,
                        IsSuccess = false,
                        Message = "Conversation Not Found"
                    };
                }
                var lastSequence = await _context.Messages
                    .Where(m => m.ConversationId == request.ConversationId)
                    .MaxAsync(m => (long?)m.SequenceNumber) ?? 0;

                var nextSequence = lastSequence + 1;
                Message message = new Message()
                {
                    Content = request.Content,
                    ConversationId = request.ConversationId,
                    MessageType = request.MessageType,
                    SenderId = _userContext.UserId,
                    SequenceNumber = nextSequence,
                };
                var participants = await _context.ConversationParticipants
                    .Where(p => p.ConversationId == request.ConversationId)
                    .ToListAsync();
                var isParticipant = participants.Any(p => p.UserId == _userContext.UserId);

                if (!isParticipant)
                {
                    return new ResultDto<ResultSendMessageDto>()
                    {
                        Data=null,
                        IsSuccess = false,
                        Message = "User And Conversation Does not Match"
                    };
                }
                var statuses = participants.Select(p => new MessageStatus
                {
                    Message = message,
                    UserId = p.UserId,
                    Status = Status.Sent,
                }).ToList();
                //MessageStatus messageStatus = new MessageStatus()
                //{
                //    Message = messagstatusese,
                //    Status = Status.Sent,
                //    UserId = request.UserId,
                //};
                await _context.Messages.AddAsync(message);
                await _context.MessagesStatuses.AddRangeAsync(statuses);
                await _context.SaveChangesAsync();
                var receivers = participants
                    .Where(p => p.UserId != _userContext.UserId)
                    .Select(p => p.UserId)
                    .ToList();
                var result = new ResultSendMessageDto()
                {
                    Content = request.Content,
                    SendDate = DateTime.Now,
                    ConversationId = request.ConversationId,
                    MessageId = message.Id,
                    SenderId = _userContext.UserId,
                };
                foreach (var userId in receivers)
                {
                    await _notificationService.SendMessageToUser(userId.ToString(), result);
                }
                return new ResultDto<ResultSendMessageDto>()
                {
                    Data = result,
                    IsSuccess = true,
                    Message = "Message Sent"
                };

            }

            else
            {
                return new ResultDto<ResultSendMessageDto>()
                {
                    Data=null,
                    IsSuccess = false,
                    Message = validationResult.ToString(),
                };
            }
        }
    }
}
