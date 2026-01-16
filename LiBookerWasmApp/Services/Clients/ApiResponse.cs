namespace LiBooker.Blazor.Client.Models
{
    public class ApiResponse<T>
    {
        public bool IsSuccess { get; set; }
        public T? Data { get; set; }
        public string? Error { get; set; }
        public int? StatusCode { get; set; }
        
        // Helper property
        public bool IsCancelled => StatusCode == 499 || Error == "Request cancelled"; // 499 is typical for Client Closed

        public static ApiResponse<T> Cancelation() 
        {
            return new ApiResponse<T> 
            { 
                IsSuccess = false, 
                Error = "Request cancelled",
                StatusCode = 499 
            };
        }
        
        public static ApiResponse<T> Success(T? data) =>
            new() { IsSuccess = true, Data = data };

        public static ApiResponse<T> Fail(string error, int? statusCode = null) =>
            new() { IsSuccess = false, Error = error, StatusCode = statusCode };
    }
}