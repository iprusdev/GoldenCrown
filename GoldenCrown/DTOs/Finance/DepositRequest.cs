using System.ComponentModel.DataAnnotations;

namespace GoldenCrown.DTOs.Finance
{
    public class DepositRequest
    {
        [Required(ErrorMessage ="поле Token обязательно")]
        public string Token {get; set;}
        [Range(0.01, double.MaxValue, ErrorMessage = "Сумма должна быть больше 0")]
        public decimal Amount { get; set; }
    }
}
