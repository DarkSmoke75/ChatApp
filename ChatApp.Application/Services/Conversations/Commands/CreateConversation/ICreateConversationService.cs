using Azure.Core;
using ChatApp.Common.Dto;
using Microsoft.EntityFrameworkCore.Update;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.Application.Services.Conversations.Commands.CreateConversation
{
    public interface ICreateConversationService
    {
        public ResultDto<long> Execute(CreateConversationDto request);
    }
}
