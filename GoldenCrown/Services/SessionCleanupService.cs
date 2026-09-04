using GoldenCrown.Data;
using Microsoft.EntityFrameworkCore;

namespace GoldenCrown.Services
{
    public class SessionCleanupService : BackgroundService
    {
        private static readonly TimeSpan CleanupInterval = TimeSpan.FromMinutes(10);

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<SessionCleanupService> _logger;

        public SessionCleanupService(
            IServiceScopeFactory scopeFactory,
            ILogger<SessionCleanupService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _scopeFactory.CreateScope();
                var dbContext = scope.ServiceProvider
                    .GetRequiredService<ApplicationDbContext>();

                var expiredSessions = await dbContext.Sessions
                    .Where(session => session.ExpiresAt <= DateTimeOffset.UtcNow)
                    .ToListAsync(stoppingToken);

                dbContext.Sessions.RemoveRange(expiredSessions);
                await dbContext.SaveChangesAsync(stoppingToken);

                _logger.LogInformation(
                    "Deleted {DeletedSessionCount} expired sessions.",
                    expiredSessions.Count);

                await Task.Delay(CleanupInterval, stoppingToken);
            }
        }
    }
}
