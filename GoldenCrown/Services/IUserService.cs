using GoldenCrown.DTOs;

namespace GoldenCrown.Services
{
    public interface IUserService
    {
        Task<RegisterResult> RegisterAsync(RegisterRequest request);
    }
}
