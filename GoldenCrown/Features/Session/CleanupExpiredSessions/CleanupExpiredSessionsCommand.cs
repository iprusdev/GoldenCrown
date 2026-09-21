using MediatR;

namespace GoldenCrown.Features.Session.CleanupExpiredSessions;

public sealed record CleanupExpiredSessionsCommand : IRequest<int>;
