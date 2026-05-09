using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Endpoint.App.Models.Dtos;
using Endpoint.App.Services.Conversation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Endpoint.App.ViewModels.Conversation
{
    public partial class MessagesViewModel:ObservableObject,IQueryAttributable
    {
        private readonly MessagesService _messagesService;
        private long conversationId;
        public ObservableCollection<MessageDto> Messages { get; } = new();
        public MessagesViewModel(MessagesService messagesService)
        {
            _messagesService = messagesService;
            
        }
        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("ConversationId", out var value))
            {
                conversationId = Convert.ToInt64(value);
            }
            GetMessages();
        }
        private async Task GetMessages()
        {
            
            var response = await _messagesService.GetMessages(conversationId);
            var responseJson = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ResultDto<List<MessageDto>>>(responseJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            if (result == null || !result.IsSuccess)
            {
                return;
            }
            foreach (var message in result.Data)
            {
                Messages.Add(message);
            }
        }

    }
}
