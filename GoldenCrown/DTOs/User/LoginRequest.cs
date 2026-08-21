using System.ComponentModel.DataAnnotations;

namespace GoldenCrown.DTOs.User
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "Поле логин обязательно")]
        [MinLength(3, ErrorMessage = "Минимальная длина логина 3 символов")]
        [MaxLength(50)]
        public string Login { get; set; }
        [Required(ErrorMessage = "Поле пароль обязательно")]
        [MinLength(6, ErrorMessage = "Минимальная длина пароля 6 символов")]
        [MaxLength(100)]
        public string Password { get; set; }

    }
}
