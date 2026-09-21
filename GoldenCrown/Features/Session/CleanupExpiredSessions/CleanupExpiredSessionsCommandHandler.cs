using GoldenCrown.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GoldenCrown.Features.Session.CleanupExpiredSessions;

public sealed class CleanupExpiredSessionsCommandHandler(ApplicationDbContext context)
    : IRequestHandler<CleanupExpiredSessionsCommand, int>
{
    public async Task<int> Handle(
        CleanupExpiredSessionsCommand request,
        CancellationToken cancellationToken)
    {
        var expiredSessions = await context.Sessions
            .Where(session => session.ExpiresAt <= DateTimeOffset.UtcNow)
            .ToListAsync(cancellationToken);

        context.Sessions.RemoveRange(expiredSessions);
        await context.SaveChangesAsync(cancellationToken);
        return expiredSessions.Count;
    }
}
