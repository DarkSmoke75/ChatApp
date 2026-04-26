using ChatApp.Application.Services.ApiClient;
using ChatApp.Application.Services.Conversations.Commands.CreateConversation;
using ChatApp.Application.Services.Conversations.Queries.GetConversations;
using ChatApp.Application.Services.Messages.Commands.SendMessage;
using ChatApp.Application.Services.Users.Commands.UserLogin;
using Endpoint.Site.Models.Dtos.Authentications;
using Endpoint.Site.Models.Dtos.Common;
using Endpoint.Site.Models.ViewModels.ConversationViewModel;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using NuGet.Protocol.Plugins;
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
                Cursor = 0
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
        public async Task<IActionResult> NewChat()
        {
            var client = _apiClientService.CreateClientWithToken(HttpContext);

            var response = await client.GetAsync("api/Users/GetUsers");

            var json = await response.Content.ReadAsStringAsync();

            var users = JsonSerializer.Deserialize<ApiResultDto<List<UsersViewModel>>>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return View(users.Data);
           
        }
        public async Task<IActionResult> CreateChat(long userId)
        {
            var client = _apiClientService.CreateClientWithToken(HttpContext);
            var requestDto = new CreateConversationDto()
            {
                IsGroup=false,
                Title=(string?) null,
                Participants = new List<CreateConversationParticipantDto>()
                {
                    new CreateConversationParticipantDto()
                    {
                        UserId=userId,
                        Role=0
                    }
                }
            };
            var json = JsonSerializer.Serialize(requestDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync($"api/Conversations/Create", content);
            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", "خطا در ایجاد مکالمه");
                return View("NewChat");
            }
            var responseJson = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ApiResultDto<long>>(
                responseJson,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }
            );
            if (result == null || !result.IsSuccess)
            {
                ModelState.AddModelError("", "خطا در ایجاد مکالمه");
                return View("NewChat");
            }
            return RedirectToAction("GetChat", new { id = result.Data });
        }
        public async Task<IActionResult> GetChat(long id)
        {
            var client = _apiClientService.CreateClientWithToken(HttpContext);
            var requestDto = new GetConversationRequestDto()
            {
                Take = 20,
                Cursor = 0
            };
            var json = JsonSerializer.Serialize(requestDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.GetAsync($"api/Messages/Get/{id}?take={requestDto.Take}");

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", "خطا در دریافت پیام ها");
                return View();
            }


            var responseJson = await response.Content.ReadAsStringAsync();

            var result = JsonSerializer.Deserialize<ApiResultDto<List<MessageViewModel>>>(
                responseJson,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }
            );

            if (result == null || !result.IsSuccess)
            {
                return View();
            }
            var chatViewModel = new ChatViewModel()
            {
                ConversationId = id,
                Messages = result.Data
            };
            return View(chatViewModel);

        }
        public async Task<IActionResult> LoadMoreMessages(long conversationId, long beforeSequence)
        {
            var client = _apiClientService.CreateClientWithToken(HttpContext);

            var response = await client.GetAsync(
                $"api/Messages/Get/{conversationId}?take=20&beforeSequence={beforeSequence}"
            );

            if (!response.IsSuccessStatusCode)
                return BadRequest();

            var responseJson = await response.Content.ReadAsStringAsync();

            return Content(responseJson, "application/json");

        }
        public async Task<IActionResult> SendMessage(MessageViewModel message)
        {

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "اطلاعات نامعتبر است";
                return RedirectToAction("GetChat", "Conversation", new { id = message.ConversationId });
            }
            var client = _apiClientService.CreateClientWithToken(HttpContext);

            var requestDto = new RequestSendMessageDto()
            {
                Content = message.Content,
                ConversationId = message.ConversationId,
                MessageType = message.MessageType,
            };


            var json = JsonSerializer.Serialize(requestDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("api/Messages/Send", content);

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", "خطا در ارسال پیام");
                TempData["Error"] = "اطلاعات نامعتبر است";
                return RedirectToAction("GetChat", "Conversation", new { id = message.ConversationId });
            }

            var responseContent = await response.Content.ReadAsStringAsync();

            var result = JsonSerializer.Deserialize<ApiResultDto<ResultSendMessageDto>>(responseContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (result == null || !result.IsSuccess)
            {
                ModelState.AddModelError("", "خطا در دریافت پیام");
                TempData["Error"] = "اطلاعات نامعتبر است";
                return RedirectToAction("GetChat", "Conversation", new { id = message.ConversationId });
            }


            return RedirectToAction("GetChat", "Conversation", new { id = message.ConversationId });
            //send message to conversation
            //return View("GetChat", message.ConversationId);
        }
    }
}
