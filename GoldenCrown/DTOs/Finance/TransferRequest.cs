using System.ComponentModel.DataAnnotations;

namespace GoldenCrown.DTOs.Finance
{
    public class TransferRequest
    {
        public string ReceiverLogin { get; set; } = string.Empty;
        public decimal Amount { get; set; }

    }
}
