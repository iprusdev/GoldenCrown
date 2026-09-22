using GoldenCrown.Models;
using MediatR;

namespace GoldenCrown.Features.Account.CreateAccount;

public sealed record CreateAccountCommand(string Login, Currency Currency) : IRequest;
