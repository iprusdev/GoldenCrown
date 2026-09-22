namespace GoldenCrown.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Login { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;

        public ICollection<Account> Accounts { get; set; } = new List<Account>();
        public Session? Session { get; set; }
        
        public ICollection<Transaction> SentTransactions { get; set; }
        = new List<Transaction>();
        public ICollection<Transaction> ReceivedTransactions { get; set; }
        = new List<Transaction>();

    }
}
