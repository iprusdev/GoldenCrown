using FluentValidation;
using GoldenCrown.DTOs.User;

namespace GoldenCrown.DTOs.Validators
{
    public class LoginRequestValidator : AbstractValidator<LoginRequest>
    {
        public LoginRequestValidator()
        {
            RuleFor(x => x.Login)
                .NotEmpty().WithMessage("Поле логин обязательно")
                .MinimumLength(3).WithMessage("Минимальная длина логина 3 символа")
                .MaximumLength(50).WithMessage("Максимальная длина логина 50 символов");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Поле пароль обязательно")  
                .MinimumLength(6).WithMessage("Минимальная длина пароля 6 символов");
        }
    }
}
