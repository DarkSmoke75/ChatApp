using Endpoint.App.Services.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace Endpoint.App.Services.Conversation
{
    public class ConversationsService
    {
    private readonly ApiClientService _apiService;
        public ConversationsService(ApiClientService apiClientService)
        {
            _apiService = apiClientService;
        }
        public async Task<HttpResponseMessage> GetConversations()
        {
            var result = await _apiService.GetAsync("/api/Conversations/Get?take=20&curosr=0");
            return result;
        }
        
    }
}
