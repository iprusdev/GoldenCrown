using GoldenCrown.Data;
using GoldenCrown.Models;
using Microsoft.EntityFrameworkCore;

namespace GoldenCrown.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAccountService _accountService;
        public UserService(ApplicationDbContext context, IAccountService accountService)
        {
            _context = context;
            _accountService = accountService;
        }
        public async Task<Result> RegisterAsync(string login, string name, string password)
        {
            //Проверка на существование пользователя с таким же логином
            var existing = await _context.Users.FirstOrDefaultAsync(u => u.Login == login);
            if (existing != null) { Result.Failure("Пользователь с таким логином уже существует"); }

            //Создание юзера
            var user = new User
            {
                Login = login,
                Name = name,
                PasswordHash = password
            };
            //Сейв юзера
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            await _accountService.CreateAccountAsync(login);
            return Result.Success() ;
        }

        public async Task<Result<string>> LoginAsync(string login, string password)
        {
            var existing = await _context.Users.FirstOrDefaultAsync(u => u.Login == login);
            if (existing == null)
            {
                return Result<string>.Failure("Invalid login or password");
            }
            if (existing.PasswordHash != password) { return null; }
            var oldSession = await _context.Sessions.FirstOrDefaultAsync(s => s.UserId == existing.Id);
            if (oldSession != null)
            {
                _context.Sessions.Remove(oldSession);
            }
            var token = Guid.NewGuid().ToString();
            var session = new Session
            {
                UserId = existing.Id,
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddHours(1)
            };
            _context.Sessions.Add(session);
            await _context.SaveChangesAsync();

            return Result<string>.Success(session.Token);
        }
    } 
}
