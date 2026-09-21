using MediatR;

namespace GoldenCrown.Features.User.UserRegister;

public sealed record UserRegisterCommand(string Login, string Name, string Password) : IRequest<Result>;
