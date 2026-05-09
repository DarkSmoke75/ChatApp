namespace Endpoint.App.Models.Dtos
{
    public class ConversationDto
    {
        public long ConversationId { get; set; }
        public string? Title { get; set; }
        public bool IsGroup { get; set; }
        public string? LastMessage { get; set; }
        public string? OtherUserName { get; set; }
        public string DisplayName { get; set; }
    }
}

