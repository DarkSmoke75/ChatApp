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
                .InclusiveBetween(1, 50);
            RuleFor(m => m.BeforeSequence).GreaterThan(0).When(m => m.BeforeSequence.HasValue);
            RuleFor(m => m.AfterSequence).GreaterThanOrEqualTo(0).When(m => m.AfterSequence.HasValue);
            RuleFor(m => m).Must(m => !m.BeforeSequence.HasValue || !m.AfterSequence.HasValue)
                .WithMessage("Use beforeSequence or afterSequence, not both.");
        }
    }
}
