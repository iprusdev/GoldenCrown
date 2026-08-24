using GoldenCrown.Data;
using Microsoft.EntityFrameworkCore;
namespace GoldenCrown.Services
{
    public class FinanceService :IFinanceService
    {
        private readonly ApplicationDbContext _context;
        public FinanceService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<decimal>> GetBalanceAsync(string token)
        { 
            var session = await _context.Sessions.FirstOrDefaultAsync(s => s.Token == token);
            if (session == null)
            {
                return Result<decimal>.Failure("Session not found");
            }
        if (session.ExpiresAt <= DateTimeOffset.UtcNow)
        {
            return Result<decimal>.Failure("Session expired");
        }
        var account = await _context.Accounts
    .FirstOrDefaultAsync(a => session.UserId == a.UserId);
        if (account == null)
        {
            return Result<decimal>.Failure("Account not found");
        }
        return Result<decimal>.Success(account.Balance);

    }
}
}