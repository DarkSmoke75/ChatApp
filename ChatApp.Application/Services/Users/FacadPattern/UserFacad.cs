using ChatApp.Application.Interfaces.Contexts;
using ChatApp.Application.Interfaces.FacadPatterns;
using ChatApp.Application.Services.Messages.Commands.SendMessage;
using ChatApp.Application.Services.Messages.Queries.GetMessage;
using ChatApp.Application.Services.Users.Commands.RegisterUser;
using ChatApp.Application.Services.Users.Commands.UserLogin;
using ChatApp.Application.Services.Users.Queries.GetRoles;
using ChatApp.Application.Services.Users.Queries.GetUsers;
using FluentValidation;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.Application.Services.Users.FacadPattern
{
    public class UserFacad : IUserFacad
    {
        private readonly IDatabaseContext _context;
        private readonly IUserContext _userContext;
        private readonly IValidator<RequestRegisterUserDto> _RegisterUserValidator;
        private readonly IValidator<RequestUserLoginDto> _UserLoginValidator;
        public UserFacad(IDatabaseContext context, IUserContext userContext,
            IValidator<RequestRegisterUserDto> registerUserValidator,
            IValidator<RequestUserLoginDto> userLoginValidator)
        {
            _context = context;
            _userContext = userContext;
            _RegisterUserValidator = registerUserValidator;
            _UserLoginValidator = userLoginValidator;
        }
        private IRegisterUserService _RegisterUserService;
        public IRegisterUserService RegisterUserService
        {
            get
            {
                return _RegisterUserService = _RegisterUserService ?? new RegisterUserService(_context, _RegisterUserValidator);
            }
        }
        private IUserLoginService _userLoginService;

        public IUserLoginService UserLoginService
        {
            get
            {
                return _userLoginService = _userLoginService ?? new UserLoginService(_context, _UserLoginValidator);
            }
        }
        private IGetRolesService _getRolesService;
        public IGetRolesService GetRolesService
        {
            get
            {
                return _getRolesService = _getRolesService ?? new GetRolesService(_context);
            }
        }
        private IGetUsersService _getUsersService;
        public IGetUsersService GetUsersService
        {
            get
            {
                return _getUsersService = _getUsersService ?? new GetUsersService(_context,_userContext);
            }
        }
    }
}
