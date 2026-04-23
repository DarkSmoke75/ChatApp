using Azure.Core;
using FluentValidation;

namespace ChatApp.Application.Services.Conversations.Commands.CreateConversation
{
    public class ConversationValidator : AbstractValidator<CreateConversationDto>
    {
        public ConversationValidator()
        {
            RuleFor(c => c.Participants)
                .NotNull().WithMessage("Participants Cant Be Null")
                .NotEmpty().WithMessage("Participants Cant Be Empty");
            RuleFor(c => c.Title)
                .NotEmpty()
                .When(c => c.IsGroup).WithMessage("Group Title Cant Be Empty");
            RuleFor(c => c.Title)
                .Empty()
                .When(c => !c.IsGroup)
                .WithMessage("Private Conversation Doesnt Accept Title");
            RuleFor(x => x).Custom((request, context) =>
            {
                var participants = request.Participants;

                if (participants == null || !participants.Any())
                    return;

                // بررسی unique بودن UserId ها
                var userIds = participants.Select(p => p.UserId).ToList();
                if (userIds.Distinct().Count() != userIds.Count)
                {
                    context.AddFailure("Participants", "Duplicate users are not allowed.");
                }

                // اگر private chat باشه
                if (!request.IsGroup)
                {
                    if (participants.Count != 1)
                    {
                        context.AddFailure("Participants", "Private conversation must have exactly 1 target user.");
                    }
                }

                else
                {
                    // اگر group باشه حداقل 2 نفر لازم داره
                    if (participants.Count < 1)
                    {
                        context.AddFailure("Participants", "Group must have at least 1 other user.");
                    }
                }
            });

        }
    }
}
