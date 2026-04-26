using ChatApp.Application.Interfaces.Contexts;
using ChatApp.Application.Interfaces.FacadPatterns;
using ChatApp.Application.Services.Conversations.Commands.CreateConversation;
using ChatApp.Application.Services.Conversations.FacadPattern;
using ChatApp.Application.Services.Conversations.Queries.GetConversations;
using ChatApp.Application.Services.Messages.Commands.SendMessage;
using ChatApp.Application.Services.Messages.Queries.GetMessage;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Endpoint.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        
        private readonly IMessageFacad _MessageFacad;
        public MessagesController(IMessageFacad messageFacad)
        {
            _MessageFacad = messageFacad;
        }
        
        [HttpGet("Get/{conversationId}")]
        public IActionResult GetMessages(long conversationId,[FromQuery] int take,[FromQuery] long? beforeSequence)
        {
            var request = new GetConversationMessagesRequestDto()
            {
                ConversationId = conversationId,
                Take = take,
                BeforeSequence = beforeSequence
            };
            var result = _MessageFacad.GetMessageService.Execute(request);
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result.Message);
            }
        }
        [HttpPost("Send")]
        public async Task<IActionResult> SendMessage([FromBody] RequestSendMessageDto request)
        {
            var result = await _MessageFacad.SendMessageService.Execute(request);
            return Ok(result);
        }
    }
}
