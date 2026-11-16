using ChatApp.Domain.Entities.Chat;

namespace Endpoint.Site.Models.ViewModels.ConversationViewModel
{
    public class MessageViewModel : ChatApp.Application.Services.Messages.MessageDto
    {
        public bool IsMine { get; set; }
    }
}
