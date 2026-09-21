using System.ComponentModel.DataAnnotations;

namespace GoldenCrown.DTOs.Finance
{
    public class TransactionHistoryRequest
    {
        public DateTimeOffset? From { get; set; }
        public DateTimeOffset? To { get; set; }
        public int Limit { get; set; }
        public int Offset { get; set; }
    }
}
