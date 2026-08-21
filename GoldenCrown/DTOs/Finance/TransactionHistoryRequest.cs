using System.ComponentModel.DataAnnotations;

namespace GoldenCrown.DTOs.Finance
{
    public class TransactionHistoryRequest
    {
        [Required(ErrorMessage = "Поле Token обязательно")]
        public string Token { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        [Range(1, int.MaxValue,ErrorMessage ="Значение limit должно быть меньше 1 ")]
        public int Limit { get; set; }
        [Range(0, int.MaxValue, ErrorMessage = "Значение offset должно быть отрицательным ")]
        public int Offset { get; set; }
    }
}
