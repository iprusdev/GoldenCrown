using MediatR;

namespace GoldenCrown.Features.User.UserLogin;

public sealed record UserLoginCommand(string Login, string Password) : IRequest<Result<string>>;
