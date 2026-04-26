using ChatApp.Common.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.Application.Services.Users.Queries.GetUsers
{
    public interface IGetUsersService
    {
        public ResultDto<List<GetUsersResultDto>> Execute();
    }
}
