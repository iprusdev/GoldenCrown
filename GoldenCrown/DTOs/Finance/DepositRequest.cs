using GoldenCrown.Models;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace GoldenCrown.DTOs.Finance
{
    public class DepositRequest
    {
        public Currency Currency { get; set; }


        public decimal Amount { get; set; }
    }
}
