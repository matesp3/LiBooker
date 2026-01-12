namespace LiBooker.Blazor.Client.Models
{
    public sealed record ApiResponse<T>
    {
        public bool IsSuccess { get; init; }
        public T? Data { get; init; }
        public string? Error { get; init; }
        public int? StatusCode { get; init; }

        public static ApiResponse<T> Success(T data) =>
            new() { IsSuccess = true, Data = data };

        public static ApiResponse<T> Fail(string error, int? statusCode = null) =>
            new() { IsSuccess = false, Error = error, StatusCode = statusCode };
    }
}