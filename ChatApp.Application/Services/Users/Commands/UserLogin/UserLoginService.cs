using ChatApp.Application.Interfaces.Contexts;
using ChatApp.Common;
using ChatApp.Common.Dto;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Application.Services.Users.Commands.UserLogin
{
    public class UserLoginService : IUserLoginService
    {
        private readonly IDatabaseContext _context;
        private readonly IValidator<RequestUserLoginDto> _validator;
        public UserLoginService(IDatabaseContext context, IValidator<RequestUserLoginDto> validator)
        {
            _context = context;
            _validator = validator;
        }
        public ResultDto<ResultUserLoginDto> Execute(RequestUserLoginDto request)
        {
            var validationResult= _validator.Validate(request);
            if(!validationResult.IsValid)
            {
                return new ResultDto<ResultUserLoginDto>()
                {
                    Data = null,
                    IsSuccess = false,
                    Message = validationResult.ToString(),
                };
            }    
            var user = _context.Users.Include(p => p.UserRoles).ThenInclude(p => p.Role).Where(p => p.Email==request.Email && p.IsActive == true).FirstOrDefault();
            if (user == null)
            {
                return new ResultDto<ResultUserLoginDto>()
                {
                    Data = null,
                    IsSuccess = false,
                    Message = "کاربر با ایمیل وارد شده یافت نشد"
                };
            }
            var passwordHasher = new PasswordHasher();
            //var hashedPassword = passwordHasher.HashPassword(password);
            bool resultVerifyPassword = passwordHasher.VerifyPassword(user.Password, request.Password);
            if (resultVerifyPassword == false)
            {
                return new ResultDto<ResultUserLoginDto>
                {
                    Data = null,
                    IsSuccess = false,
                    Message = "رمز عبور وارد شده صحیح نمی باشد"
                };
            }
            List<string> roles = new List<string>();
            roles = user.UserRoles.Select(r => r.Role.Name).ToList();
            return new ResultDto<ResultUserLoginDto>()
            {
                Data = new ResultUserLoginDto()
                {
                    Roles = roles,
                    UserId = user.Id,
                    DisplayName= user.DisplayName,
                    Username=user.Username,
                },
                IsSuccess = true,
                Message = "ورود با موفقیت انجام شد"
            };
        }
    }
}
