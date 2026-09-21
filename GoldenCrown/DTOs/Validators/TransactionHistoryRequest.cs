using FluentValidation;
using GoldenCrown.DTOs.Finance;

namespace GoldenCrown.DTOs.Validators
{
    public class TransactionHistoryRequestValidator:AbstractValidator<TransactionHistoryRequest>
    {
        public  TransactionHistoryRequestValidator() { 
            RuleFor(x => x.From)
                .LessThanOrEqualTo(x => x.To)
                .When(x => x.From.HasValue && x.To.HasValue)
                .WithMessage("Дата начала не должна быть позже даты окончания");
            RuleFor(x => x.Offset)
                .GreaterThanOrEqualTo(0).WithMessage("Смещение должно быть неотрицательным");
            RuleFor(x => x.Limit)
                .GreaterThan(0).WithMessage("Лимит должен быть положительным");
        }
    }
}
