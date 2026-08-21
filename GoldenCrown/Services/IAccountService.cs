using System.Runtime.CompilerServices;

namespace GoldenCrown.Services
{
    public interface IAccountService
    {
        Task CreateAccountAsync(string login);
    }
}
