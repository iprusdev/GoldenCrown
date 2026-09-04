using System.ComponentModel.DataAnnotations;

namespace GoldenCrown.DTOs.Finance
{
    public class TransferRequest
    {



        [Required(ErrorMessage = "Поле ReceiverLogin обязательно")]
        public string ReceiverLogin { get; set; }
        [Range(0.01,double.MaxValue,ErrorMessage ="Сумма должна быть больше 0")]
        public decimal Amount { get; set; }

    }
}
