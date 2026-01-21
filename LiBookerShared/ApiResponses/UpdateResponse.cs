namespace LiBookerShared.ApiResponses
{
    public class UpdateResponse<T>
    {
        public T? UpdatedDto { get; set; }
        public bool IsSuccess { get; set; }

        public string? ErrorMessage { get; set; }

        public static UpdateResponse<T> Success(T updatedDto)
        {
            return new UpdateResponse<T>
            {
                UpdatedDto = updatedDto,
                IsSuccess = true,
                ErrorMessage = null
            };
        }

        public static UpdateResponse<T> Failure(string errorMessage)
        {
            return new UpdateResponse<T>
            {
                UpdatedDto = default,
                IsSuccess = false,
                ErrorMessage = errorMessage
            };
        }
    }
}
