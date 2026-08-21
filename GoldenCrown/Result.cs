namespace GoldenCrown
{
    public class Result<T>:Result
    {
        public T Value { get; set; }
        public static Result<T> Success(T value) => new Result<T> { Value = value, IsSuccess = true };
        public static new Result<T> Failure(string errorMessage) => new Result<T> { ErrorMessage = errorMessage, IsSuccess = false };
    }
    public class Result
    {

        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
        public static Result Success() => new Result{ IsSuccess = true };
        public static  Result Failure(string errorMessage) => new Result { ErrorMessage = errorMessage, IsSuccess = false };
        public static implicit operator bool (Result result) => result.IsSuccess;
    }
}
