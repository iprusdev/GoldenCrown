namespace GoldenCrown.Models
{
    public class Transaction
    {
        public long Id { get; set; }
        public int? SenderId { get; set; }
        public int ReceiverId { get; set; }
        public DateTimeOffset Date { get; set; }
        public decimal Amount { get; set; }

        public User? Sender { get; set; }
        public User Receiver { get; set; } = null!;
    }
}
