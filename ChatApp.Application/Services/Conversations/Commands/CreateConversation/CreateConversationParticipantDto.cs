using ChatApp.Domain.Entities.Chat;

namespace ChatApp.Application.Services.Conversations.Commands.CreateConversation
{
    public class CreateConversationParticipantDto
    {
        public long UserId { get; set; }
        public DateTime? LeftTime { get; set; }
        public ParticipantRole Role { get; set; }
    }
}
