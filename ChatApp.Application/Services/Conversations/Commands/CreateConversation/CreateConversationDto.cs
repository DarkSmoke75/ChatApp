namespace ChatApp.Application.Services.Conversations.Commands.CreateConversation
{
    public class CreateConversationDto
    {
        public string? Title { get; set; }
        public bool IsGroup { get; set; }
        public List<CreateConversationParticipantDto> Participants { get; set; }

    }
}
