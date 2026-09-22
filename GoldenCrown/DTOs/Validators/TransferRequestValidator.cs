using FluentValidation;
using GoldenCrown.DTOs.Finance;

namespace GoldenCrown.DTOs.Validators
{
    public class TransferRequestValidator:AbstractValidator<TransferRequest>
    {
        public TransferRequestValidator()
        {
            RuleFor(x => x.Currency).IsInEnum().WithMessage("Укажите валюту USD, EUR или BYN");

            RuleFor(x => x.ReceiverLogin)
                .NotEmpty().WithMessage("Поле логин получателя обязательно")
                .MinimumLength(3).WithMessage("Минимальная длина логина получателя 3 символа")
                .MaximumLength(50).WithMessage("Максимальная длина логина получателя 50 символов");
            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("Сумма должна быть больше 0");
        }
    }
}
