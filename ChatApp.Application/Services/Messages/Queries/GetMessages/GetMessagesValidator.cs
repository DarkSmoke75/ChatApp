using FluentValidation;

namespace ChatApp.Application.Services.Messages.Queries.GetMessage
{
    public class GetMessagesValidator : AbstractValidator<GetConversationMessagesRequestDto>
    {
        public GetMessagesValidator()
        {
            RuleFor(m => m.ConversationId)
                .GreaterThan(0).WithMessage("ConversationId is Invalid)");
            RuleFor(m => m.Take)
                .LessThanOrEqualTo(50).WithMessage("Cant Take More Than 50 Messages");
        }
    }
}
