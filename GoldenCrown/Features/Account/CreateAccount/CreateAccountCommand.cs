using MediatR;

namespace GoldenCrown.Features.Account.CreateAccount;

public sealed record CreateAccountCommand(string Login) : IRequest;
