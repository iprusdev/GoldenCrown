using GoldenCrown.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GoldenCrown.Features.Finance.Transfer;

public sealed class TransferCommandHandler(ApplicationDbContext context)
    : IRequestHandler<TransferCommand, Result>
{
    public async Task<Result> Handle(TransferCommand request, CancellationToken cancellationToken)
    {
        if (!Enum.IsDefined(request.Currency))
        {
            return Result.Failure("Укажите валюту USD, EUR или BYN");
        }

        if (request.Amount <= 0)
        {
            return Result.Failure("Сумма должна быть больше нуля");
        }

        var sender = await context.Users.FirstOrDefaultAsync(a => a.Id == request.FromUserId, cancellationToken);
        if (sender == null)
        {
            return Result.Failure("Отправитель не найден");
        }

        var senderAccount = await context.Accounts.FirstOrDefaultAsync(a => a.UserId == sender.Id && a.Currency == request.Currency, cancellationToken);
        if (senderAccount == null)
        {
            return Result.Failure("Счёт отправителя не найден");
        }

        var receiver = await context.Users.FirstOrDefaultAsync(a => a.Login == request.ReceiverLogin, cancellationToken);
        if (receiver == null)
        {
            return Result.Failure("Получатель не найден");
        }

        if (receiver.Id == request.FromUserId)
        {
            return Result.Failure("Нельзя перевести средства самому себе");
        }

        var receiverAccount = await context.Accounts.FirstOrDefaultAsync(a => a.UserId == receiver.Id && a.Currency == request.Currency, cancellationToken);
        if (receiverAccount == null)
        {
            return Result.Failure("Счёт получателя не найден");
        }

        if (senderAccount.Balance < request.Amount)
        {
            return Result.Failure("Недостаточно средств");
        }

        senderAccount.Balance -= request.Amount;
        receiverAccount.Balance += request.Amount;
        var transaction = new GoldenCrown.Models.Transaction
        {
            SenderId = senderAccount.UserId,
            ReceiverId = receiverAccount.UserId,
            Amount = request.Amount, Currency = request.Currency,
            Date = DateTimeOffset.UtcNow
        };
        context.Add(transaction);
        await context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
