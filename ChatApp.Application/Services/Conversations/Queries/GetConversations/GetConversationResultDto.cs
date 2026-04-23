namespace ChatApp.Application.Services.Conversations.Queries.GetConversations
{
    public class GetConversationResultDto
    {
        public long ConversationId { get; set; }

        public string Title { get; set; }

        public bool IsGroup { get; set; }

        public string LastMessage { get; set; }

        public long? LastMessageSequence { get; set; }

        public DateTime? LastMessageTime { get; set; }

        //public int UnreadCount { get; set; }

        public long? OtherUserId { get; set; }
        public string? OtherUserName { get; set; }
    }
    //public class GetConversationsValidator : AbstractValidator<GetConversationRequestDto>
    //{
    //    public GetConversationsValidator() 
    //    {
           
    //    }
    //}

}
