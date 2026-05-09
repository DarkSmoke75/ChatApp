using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Endpoint.App.Models.Dtos;
using Endpoint.App.Services.Conversation;
using Endpoint.App.Services.Navigations;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Endpoint.App.ViewModels.Conversation
{
    public partial class ConversationViewModel : ObservableObject
    {
        [ObservableProperty] private string conversationId;
        [ObservableProperty] private string title;
        [ObservableProperty] private string lastMessage;
        public ObservableCollection<ConversationDto> Conversations { get; }=new();
        
        private readonly ConversationsService _conversationsService;
        private readonly NavigationService _navigationService;
        public ConversationViewModel(ConversationsService conversationsService,NavigationService navigationService)
        {
            _conversationsService = conversationsService;
            _navigationService = navigationService;
        }
        [RelayCommand]
        private async Task GetConversations()
        {
            var response = await _conversationsService.GetConversations();
            var responseJson = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ResultDto<List<ConversationDto>>>(responseJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });


            if (result == null || !result.IsSuccess)
            {
                return;
            }
            foreach (var con in result.Data)
            {
                con.DisplayName = con.Title ?? con.OtherUserName;
                Conversations.Add(con);
            }
            //var chatViewModel = new ChatViewModel()
            //{
            //    ConversationId = id,
            //    Messages = result.Data
            //};
            //var conversations = 
        }
        [RelayCommand]private async Task OpenConversation(long conversationId)
        {
            if (conversationId == null)
                return;
            
            await Shell.Current.GoToAsync("Messages", new Dictionary<string, object>
            {
                ["ConversationId"] = conversationId
            });
        }
    }
}
