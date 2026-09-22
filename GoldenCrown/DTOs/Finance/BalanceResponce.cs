using GoldenCrown.Models;
namespace GoldenCrown.DTOs.Finance
{
    public class BalanceResponce
    {
        public Currency Currency { get; set; }

        public decimal Balance { get; set; }
    }
}
