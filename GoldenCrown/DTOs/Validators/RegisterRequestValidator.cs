using FluentValidation;
using GoldenCrown.DTOs.User;

namespace GoldenCrown.DTOs.Validators
{
    public class RegisterRequestValidator:AbstractValidator<RegisterRequest>
    {
        public RegisterRequestValidator()
        {
            RuleFor(x => x.Login)
                .NotEmpty().WithMessage("Поле логин обязательно")
                .MinimumLength(3).WithMessage("Минимальная длина логина 3 символа")
                .MaximumLength(50).WithMessage("Максимальная длина логина 50 символов");
            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Поле пароль обязательно")
                .MinimumLength(6).WithMessage("Минимальная длина пароля 6 символов");
            RuleFor(x=> x.Name)
                .NotEmpty().WithMessage("Поле имя обязательно")
                .MinimumLength(2).WithMessage("Минимальная длина имени 2 символа")
                .MaximumLength(100).WithMessage("Максимальная длина имени 100 символов");
        }
    }
}
