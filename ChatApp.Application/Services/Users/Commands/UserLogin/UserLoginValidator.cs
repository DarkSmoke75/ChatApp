using FluentValidation;
using static ChatApp.Application.Services.Users.Commands.UserLogin.UserLoginService;

namespace ChatApp.Application.Services.Users.Commands.UserLogin
{
    public class UserLoginValidator:AbstractValidator<RequestUserLoginDto>
    {
        public UserLoginValidator()
        {
            RuleFor(p => p.Email)
                .NotEmpty().WithMessage("Email is Requiered");
            RuleFor(p => p.Password)
                .NotEmpty().WithMessage("Password is Requiered")
                .MinimumLength(8).WithMessage("Password Must Be at Least 8 Characters");
        }
    }
}
