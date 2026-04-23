using ChatApp.Common.Dto;
using ChatApp.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore.Update;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.Application.Services.Messages.Commands.SendMessage
{
    public interface ISendMessageService
    {
        public Task<ResultDto<ResultSendMessageDto>> Execute(RequestSendMessageDto request);
    }
}
