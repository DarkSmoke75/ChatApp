using ChatApp.Common.Dto;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.Application.Services.Messages.Queries.GetMessage
{
    public interface IGetMessagesService
    {
        public ResultDto<List<GetConversationMessagesResultDto>> Execute(GetConversationMessagesRequestDto request);
    }
}
