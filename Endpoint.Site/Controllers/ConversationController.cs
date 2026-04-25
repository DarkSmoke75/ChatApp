using ChatApp.Application.Services.ApiClient;
using ChatApp.Application.Services.Conversations.Queries.GetConversations;
using Endpoint.Site.Models.Dtos.Common;
using Endpoint.Site.Models.ViewModels.ConversationViewModel;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace Endpoint.Site.Controllers
{
    public class ConversationController : Controller
    {
        private readonly IWebHostEnvironment _environment;
        private readonly IApiClientService _apiClientService;
        public ConversationController(IWebHostEnvironment environment, IApiClientService apiClientService)
        {
            _environment = environment;
            _apiClientService = apiClientService;
        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            //get conversations from api and pass to view
            var client = _apiClientService.CreateClientWithToken(HttpContext);
            var requestDto = new GetConversationRequestDto()
            {
                Take = 20,
                Cursor=0
            };
            var json = JsonSerializer.Serialize(requestDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.GetAsync($"api/Conversations/Get?take={requestDto.Take}&cursor={requestDto.Cursor}");

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", "خطا در دریافت مکالمات");
                return View();
            }
            if (!response.IsSuccessStatusCode)
            {
                return View("Error");
            }

            var responseJson = await response.Content.ReadAsStringAsync();

            var result = JsonSerializer.Deserialize<ApiResultDto<List<ConversationViewModel>>>(
                responseJson,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }
            );

            if (result == null || !result.IsSuccess)
            {
                return View("Error");
            }

            return View(result.Data);
            
        }
        public async Task<IActionResult> Create()
        {
            //create new conversation
            return View();
        }
        public async Task<IActionResult> Chat(int id)
        {
            //get conversation details from api and pass to view
            return View();
        }
        public async Task<IActionResult> SendMessage(int id)
        {
            //send message to conversation
            return View();
        }
    }
}
