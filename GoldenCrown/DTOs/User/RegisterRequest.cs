using System.ComponentModel.DataAnnotations;

namespace GoldenCrown.DTOs.User
{
    public class RegisterRequest
    {
        public string Login { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}