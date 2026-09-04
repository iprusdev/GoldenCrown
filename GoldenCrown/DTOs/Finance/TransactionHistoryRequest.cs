using System.ComponentModel.DataAnnotations;

namespace GoldenCrown.DTOs.Finance
{
    public class TransactionHistoryRequest
    {
        public DateTimeOffset? From { get; set; }
        public DateTimeOffset? To { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Значение limit должно быть не меньше 1")]
        public int Limit { get; set; }
        [Range(0, int.MaxValue, ErrorMessage = "Значение offset не может быть отрицательным")]
        public int Offset { get; set; }
    }
}
