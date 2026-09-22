using GoldenCrown.Models;
using System.ComponentModel.DataAnnotations;

namespace GoldenCrown.DTOs.Finance
{
    public class TransactionHistoryResponse
    {
        public Currency Currency { get; set; }

        public string? SenderName { get; set; }
        public string ReceiverName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTimeOffset Date { get; set; }
    }
}
