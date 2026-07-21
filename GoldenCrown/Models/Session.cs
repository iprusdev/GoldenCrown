namespace GoldenCrown.Models
{
    public class Session
    {
        public int UserId { get; set; }

        public string Token { get; set; } = string.Empty;

        public DateTimeOffset ExpiresAt { get; set; }

        public User User { get; set; } = null!;

    }
}
