using GoldenCrown.Data;
using GoldenCrown.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GoldenCrown.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAccountService _accountService;
        private readonly IPasswordHasher<User> _passwordHasher;

        public UserService(
            ApplicationDbContext context,
            IAccountService accountService,
            IPasswordHasher<User> passwordHasher)
        {
            _context = context;
            _accountService = accountService;
            _passwordHasher = passwordHasher;
        }
        public async Task<Result> RegisterAsync(string login, string name, string password)
        {
            //Проверка на существование пользователя с таким же логином
            var existing = await _context.Users.FirstOrDefaultAsync(u => u.Login == login);
            if (existing != null)
            {
                return Result.Failure("Пользователь с таким логином уже существует");
            }

            //Создание юзера
            var user = new User
            {
                Login = login,
                Name = name
            };
            user.PasswordHash = _passwordHasher.HashPassword(user, password);
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
            var passwordVerification = VerifyPassword(existing, password);
            if (passwordVerification == PasswordVerificationResult.Failed)
            {
                return Result<string>.Failure("Invalid login or password");
            }

            if (passwordVerification == PasswordVerificationResult.SuccessRehashNeeded)
            {
                existing.PasswordHash = _passwordHasher.HashPassword(existing, password);
            }
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

        private PasswordVerificationResult VerifyPassword(User user, string password)
        {
            try
            {
                var result = _passwordHasher.VerifyHashedPassword(
                    user,
                    user.PasswordHash,
                    password);

                if (result != PasswordVerificationResult.Failed)
                {
                    return result;
                }
            }
            catch (FormatException)
            {
                // Older application versions stored passwords without hashing.
            }

            if (user.PasswordHash != password)
            {
                return PasswordVerificationResult.Failed;
            }

            return PasswordVerificationResult.SuccessRehashNeeded;
        }
    } 
}
