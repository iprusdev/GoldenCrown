using GoldenCrown.Models;

namespace GoldenCrown.DTOs.Finance;

public sealed record AccountResponse(int Id, Currency Currency, decimal Balance);
