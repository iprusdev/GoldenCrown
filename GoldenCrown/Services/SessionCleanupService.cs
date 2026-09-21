using GoldenCrown.Features.Session.CleanupExpiredSessions;
using MediatR;

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
                var sender = scope.ServiceProvider.GetRequiredService<ISender>();
                var deletedSessionCount = await sender.Send(
                    new CleanupExpiredSessionsCommand(), stoppingToken);
                _logger.LogInformation(
                    "Deleted {DeletedSessionCount} expired sessions.",
                    deletedSessionCount);

                await Task.Delay(CleanupInterval, stoppingToken);
            }
        }
    }
}
