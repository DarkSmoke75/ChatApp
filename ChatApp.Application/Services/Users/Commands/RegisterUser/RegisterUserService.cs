using ChatApp.Application.Interfaces.Contexts;
using ChatApp.Common;
using ChatApp.Common.Dto;
using ChatApp.Domain.Entities.Users;
using FluentValidation;
using System.Text.RegularExpressions;

namespace ChatApp.Application.Services.Users.Commands.RegisterUser
{
    public class RegisterUserService : IRegisterUserService
    {
        private readonly IDatabaseContext _Context;
        private readonly IValidator<RequestRegisterUserDto> _validator;
        public RegisterUserService(IDatabaseContext context, IValidator<RequestRegisterUserDto> validator)
        {
            _Context = context;
            _validator = validator;
        }
        public ResultDto<ResultRegisterUserDto> Execute(RequestRegisterUserDto request)
        {

            var validationResult = _validator.Validate(request);
            if (!validationResult.IsValid)
            {
                return new ResultDto<ResultRegisterUserDto>()
                {
                    IsSuccess = false,
                    Message = validationResult.ToString(),
                };
            }
            var emailExists = _Context.Users.Any(u => u.Email == request.Email);
            if (emailExists)
            {
                return new ResultDto<ResultRegisterUserDto>()
                {
                    IsSuccess = false,
                    Message = "این ایمیل قبلا ثبت شده است"
                };
            }
            var usernameExists = _Context.Users.Any(u => u.Username == request.Username);
            if (usernameExists)
            {
                return new ResultDto<ResultRegisterUserDto>()
                {
                    IsSuccess = false,
                    Message = "این نام کاربری قبلا ثبت شده است"
                };
            }
            
            var passwordHasher = new PasswordHasher();
            var hashedPassword = passwordHasher.HashPassword(request.Password);
            User user = new User()
            {
                Email = request.Email,
                Password = hashedPassword,
                DisplayName = request.DisplayName,
                Username = request.Username,
                IsActive = true,
            };
            List<UserInRole> userInRoles = new List<UserInRole>();

            var role = _Context.Roles.FirstOrDefault(r => r.Name == "User");
            if (role == null)
            {
                return new ResultDto<ResultRegisterUserDto>()
                {
                    IsSuccess = false,
                    Message = "Role معتبر نیست"
                };
            }
            else
            {
                userInRoles.Add(new UserInRole { Role = role, RoleId = role.Id, User = user, UserId = user.Id });
            }
            user.UserRoles = userInRoles;
            _Context.Users.Add(user);
            _Context.SaveChanges();
            return new ResultDto<ResultRegisterUserDto>()
            {
                Data = new ResultRegisterUserDto()
                { UserId = user.Id, },
                IsSuccess = true
                ,
                Message = "ثبت نام کاربر با موفقیت انجام شد",
            };

        }
    }
}
