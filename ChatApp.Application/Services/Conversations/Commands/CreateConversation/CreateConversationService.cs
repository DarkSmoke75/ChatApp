using ChatApp.Application.Interfaces.Contexts;
using ChatApp.Common.Dto;
using ChatApp.Domain.Entities.Chat;
using FluentValidation;

namespace ChatApp.Application.Services.Conversations.Commands.CreateConversation
{
    public class CreateConversationService : ICreateConversationService
    {
        private readonly IDatabaseContext _context;
        private readonly IUserContext _userContext;
        private readonly IValidator<CreateConversationDto> _validator;
        public CreateConversationService(IDatabaseContext context, IValidator<CreateConversationDto> validatior, IUserContext userContext)
        {
            _context = context;
            _validator = validatior;
            _userContext = userContext;
        }
        public ResultDto Execute(CreateConversationDto request)
        {
            var validationResult = _validator.Validate(request);
            if (validationResult.IsValid)
            {
                var currentUserId = _userContext.UserId;

                var targetUserId = request.Participants.First().UserId;

                // ❗ چک وجود چت قبلی
                var existingConversation = _context.Conversations
                    .Where(c => !c.IsGroup && c.Participants.Count == 2)
                    .Where(c => c.Participants.Any(p => p.UserId == currentUserId))
                    .Where(c => c.Participants.Any(p => p.UserId == targetUserId))
                    .FirstOrDefault();

                if (existingConversation != null)
                {
                    return new ResultDto
                    {
                        IsSuccess = true,
                        Message = "Conversation already exists.",
                    };
                }

                Conversation conversation = new Conversation()
                {
                    IsGroup = request.IsGroup,
                    Title = request.Title,
                };
                _context.Conversations.Add(conversation);
                List<ConversationParticipant> conversationParticipants = new List<ConversationParticipant>();
                var distinctUserIds = request.Participants
                    .Select(p => p.UserId)
                    .Distinct()
                    .ToList();
                if (distinctUserIds.Count != request.Participants.Count)
                {
                    return new ResultDto
                    {
                        IsSuccess = false,
                        Message = "Duplicate users are not allowed."
                    };
                }
                if (distinctUserIds.Contains(currentUserId))
                {
                    return new ResultDto
                    {
                        IsSuccess = false,
                        Message = "You cannot chat with yourself."
                    };
                }
                conversationParticipants = request.Participants
                    .Select(p => new ConversationParticipant
                    {
                        JoinDate = DateTime.UtcNow,
                        ParticipantRole = p.Role,
                        Conversation = conversation,
                        UserId = p.UserId,
                    })
                    .ToList();

                // اضافه کردن یوزر فعلی
                conversationParticipants.Add(new ConversationParticipant
                {
                    JoinDate = DateTime.UtcNow,
                    ParticipantRole = ParticipantRole.Member,
                    Conversation = conversation,
                    UserId = currentUserId,
                });
                //foreach (var participant in request.Participants)
                //{
                //    conversationParticipants.Add(
                //        new ConversationParticipant
                //        {
                //            JoinDate = DateTime.UtcNow,
                //            ParticipantRole = participant.Role,
                //            Conversation = conversation,
                //            UserId = participant.UserId,
                //        });
                //}
                //conversationParticipants.Add(
                //    new ConversationParticipant
                //    {
                //        JoinDate = DateTime.UtcNow,
                //        ParticipantRole = ParticipantRole.Member,
                //        Conversation = conversation,
                //        UserId = currentUserId,
                //    });
                _context.ConversationParticipants.AddRange(conversationParticipants);
                _context.SaveChanges();
                return new ResultDto()
                {
                    IsSuccess = true,
                    Message = "Conversation Created Successfully"
                };
            }
            else
            {
                return new ResultDto()
                {
                    IsSuccess = false,
                    Message = validationResult.ToString()
                };
            }
        }
    }
}
