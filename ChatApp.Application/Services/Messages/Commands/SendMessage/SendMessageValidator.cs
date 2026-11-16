using FluentValidation;

namespace ChatApp.Application.Services.Messages.Commands.SendMessage
{
    public class SendMessageValidator : AbstractValidator<RequestSendMessageDto>
    {

        public SendMessageValidator()
        {
            RuleFor(m => m.ClientRequestId).NotEmpty();
            RuleFor(m => m.MessageType).Equal(ChatApp.Domain.Entities.Chat.MessageType.Text)
                .WithMessage("Only text messages are supported.");

            RuleFor(m => m.ConversationId)
                .GreaterThan(0).WithMessage("ConversationId is Invalid)");
            RuleFor(m => m.Content)
                .NotEmpty().WithMessage("Message Cant Be Empty")
                .NotNull().WithMessage("Message Cant Be Null")
                .MaximumLength(10000);

        }
    }
}
