namespace LiBooker.Shared.ApiResponses
{
    public class SimpleResponse
    {
        public required bool Success { get; set; }
        public string? Message { get; set; } = string.Empty;
    }
}
