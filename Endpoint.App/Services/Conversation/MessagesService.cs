using Endpoint.App.Services.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Endpoint.App.Services.Conversation
{
    public class MessagesService
    {
        protected readonly ApiClientService _apiService;
        public MessagesService(ApiClientService apiService)
        {
            _apiService = apiService;
        }
        public async Task<HttpResponseMessage> GetMessages(long conversationId)
        {
            var result = await _apiService.GetAsync($"/api/Messages/Get/{conversationId}?take=20");
         
            return result;
        }
    }
}
