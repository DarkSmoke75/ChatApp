using ChatApp.Application.Services.Messages.Queries.GetMessage;
using ChatApp.Common.Dto;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.Application.Services.Conversations.Queries.GetConversations
{
    public interface IGetConversationsService
    {
        public ResultDto<List<GetConversationResultDto>> Execute(GetConversationRequestDto request);
    }
    //public class GetConversationsValidator : AbstractValidator<GetConversationRequestDto>
    //{
    //    public GetConversationsValidator() 
    //    {
           
    //    }
    //}

}
