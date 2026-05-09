using ChatApp.Application.Interfaces.Contexts;
using ChatApp.Application.Interfaces.FacadPatterns;
using ChatApp.Application.Services.Conversations.Commands.CreateConversation;
using ChatApp.Application.Services.Conversations.Queries.GetConversations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Endpoint.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ConversationsController : ControllerBase
    {
        
        private readonly IConversationFacad _conversationFacad;
        public ConversationsController(IConversationFacad conversationFacad)
        {
            _conversationFacad = conversationFacad;
        }
        [HttpGet("Get")]
        public IActionResult GetConversations([FromQuery] int take, [FromQuery] long? cursor)
        {
            var request = new GetConversationRequestDto()
            {
                Cursor = cursor,
                Take = take
            };
            var result = _conversationFacad.GetConversationsService.Execute(request);
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result.Message);
            }
        }
        [HttpPost("Create")]
        public IActionResult CreateConversation([FromBody] CreateConversationDto request)
        {
            var result = _conversationFacad.CreateConversationService.Execute(request);
            return Ok(result);
        }
    }
}
