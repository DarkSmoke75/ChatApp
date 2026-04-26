using ChatApp.Application.Services.Users.Commands.RegisterUser;
using ChatApp.Application.Services.Users.Commands.UserLogin;
using ChatApp.Application.Services.Users.Queries.GetRoles;
using ChatApp.Application.Services.Users.Queries.GetUsers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.Application.Interfaces.FacadPatterns
{
    public interface IUserFacad
    {
        IRegisterUserService RegisterUserService { get; }
        IUserLoginService UserLoginService { get; }  
        IGetRolesService GetRolesService { get; }
        IGetUsersService GetUsersService { get; }
    }
}
