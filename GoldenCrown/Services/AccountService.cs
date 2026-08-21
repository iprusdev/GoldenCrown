using GoldenCrown.Data;
using GoldenCrown.Models;
using Microsoft.EntityFrameworkCore;

namespace GoldenCrown.Services
{
    public class AccountService : IAccountService
    {
        private readonly ApplicationDbContext _context;
        public AccountService(ApplicationDbContext context)
        {
            _context = context;
        }

        //Создать новый счет для пользователя с балансом 0
        //Сохранить в базу данных
        public async Task CreateAccountAsync (string login)
        {
            var user  = await _context.Users.FirstOrDefaultAsync (x => x.Login == login);
            if(user == null) 
            {
                throw new InvalidOperationException($"Unable to find user with login {login}");
            }

            var account = new Account()
            { 
                UserId = user.Id,
                Balance = 0 
            };
            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();
            
        }
        


    }
}
