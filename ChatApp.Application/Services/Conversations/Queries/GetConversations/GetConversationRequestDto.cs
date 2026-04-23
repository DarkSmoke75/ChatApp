namespace ChatApp.Application.Services.Conversations.Queries.GetConversations
{
    public class GetConversationRequestDto
    {
        public int Take { get; set; } = 20;

        public long? Cursor { get; set; }

        //public bool? IsGroup { get; set; }
    }
    //public class GetConversationsValidator : AbstractValidator<GetConversationRequestDto>
    //{
    //    public GetConversationsValidator() 
    //    {
           
    //    }
    //}

}
