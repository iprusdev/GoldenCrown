namespace GoldenCrown.Services
{
    public record RegisterResult(
        bool IsSuccess,
        string? ErrorMessage);
}
