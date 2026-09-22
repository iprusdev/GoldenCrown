using FluentValidation;
using GoldenCrown.DTOs.Finance;

namespace GoldenCrown.DTOs.Validators
{
    public class DepositRequestValidator:AbstractValidator<DepositRequest>
    {
        public DepositRequestValidator()
        {
            RuleFor(x => x.Currency).IsInEnum().WithMessage("Укажите валюту USD, EUR или BYN");

            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("Сумма должна быть больше 0");
        }
    }
}
