using FluentValidation;

namespace ChatApp.Application.Services.Users.Commands.RegisterUser
{
    public class RegisterUserValidator : AbstractValidator<RequestRegisterUserDto>
    {
        public RegisterUserValidator()
        {
            RuleFor(p => p.Username).NotEmpty().Length(3, 32)
                .Matches(@"^[\p{L}\p{N}_]+$")
                .WithMessage("Username must contain 3–32 letters, numbers or underscores.");
            RuleFor(p => p.Email)
                .NotEmpty().WithMessage("Email Cant Be Empty")
                .NotNull().WithMessage("Email Cant Be Null")
                .EmailAddress().WithMessage("Email Is Not Valid");
            RuleFor(p => p.DisplayName)
                .NotEmpty().WithMessage("Display Name Cant Be Empty")
                .NotNull().WithMessage("Display Name Cant Be Null");
            RuleFor(p => p.Password)
                .NotEmpty().WithMessage("Password Cant Be Empty")
                .NotNull().WithMessage("Password Cant Be Null");
            RuleFor(p => p.Password)
                .Equal(p => p.RePassword).WithMessage("Password Does not match");
            RuleFor(p => p.Password)
                .MinimumLength(8)
                .WithMessage("Password must be at least 8 characters");
        }
    }
}
