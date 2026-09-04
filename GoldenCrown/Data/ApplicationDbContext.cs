using GoldenCrown.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using static System.Collections.Specialized.BitVector32;

namespace GoldenCrown.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<Account> Accounts => Set<Account>();
        public DbSet<Session> Sessions => Set<Session>();
        public DbSet<Transaction> Transactions => Set<Transaction>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<User>()
                .HasOne(user => user.Account)
                .WithOne(account => account.User)
                .HasForeignKey<Account>(account => account.UserId);
            modelBuilder.Entity<Session>()
                .HasKey(session => session.UserId);
            modelBuilder.Entity<User>()
                .HasOne(user => user.Session)
                .WithOne(session => session.User)
                .HasForeignKey<Session>(session => session.UserId);
            modelBuilder.Entity<Transaction>()
                .HasOne(transaction => transaction.Sender)
                .WithMany(user => user.SentTransactions)
                .HasForeignKey(transaction => transaction.SenderId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Transaction>()
                .HasOne(transaction => transaction.Receiver)
                .WithMany(user => user.ReceivedTransactions)
                .HasForeignKey(transaction => transaction.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<User>()
                .Property(user => user.Login)
                .HasMaxLength(50)
                .IsRequired();

            modelBuilder.Entity<User>()
                .HasIndex(user => user.Login)
                .IsUnique();

            modelBuilder.Entity<User>()
                .Property(user => user.Name)
                .HasMaxLength(100)
                .IsRequired();

            modelBuilder.Entity<User>()
                .Property(user => user.PasswordHash)
                .HasMaxLength(500)
                .IsRequired();

            modelBuilder.Entity<Session>()
                .Property(session => session.Token)
                .HasMaxLength(100)
                .IsRequired();

            modelBuilder.Entity<Session>()
                .HasIndex(session => session.Token)
                .IsUnique();

            modelBuilder.Entity<Account>()
                .Property(account => account.Balance)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Transaction>()
                .Property(transaction => transaction.Amount)
                .HasPrecision(18, 2);


           modelBuilder.Entity<User>().HasData(
         new
         {
             Id = 1,
             Login = "testuser1",
             Name = "Test User 1",
             PasswordHash = "AQAAAAIAAYagAAAAEJNvqd+NoFqelrNYynzWs+Eqa2zJ4J6GN+pAKRvspk38g4V3UTB1jHNdgO/EZUsP2Q=="
         },
         new
         {
             Id = 2,
             Login = "testuser2",
             Name = "Test User 2",
             PasswordHash = "AQAAAAIAAYagAAAAEInhvCnFwcUP+83KkhVErZskd9Wvyo/j3bnUILGdt7Ta2x3NPqD2wpACyH8IZ4E2SA=="
         },
         new
         {
             Id = 3,
             Login = "testuser3",
             Name = "Test User 3",
             PasswordHash = "AQAAAAIAAYagAAAAEGYzKMpM9Dtvit54wAx0NYpKHdvlqXggThin4CelAx2OWQxgpEPzNzfr7nQ+vbxuMA=="
         }
);
        }
    }
}
