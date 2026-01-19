using System.Text.Json.Serialization;

namespace LiBooker.Shared.DTOs
{
    // Response from Microsoft Identity API (/login)
    public class AuthResponse
    {
        [JsonPropertyName("tokenType")]
        public string TokenType { get; set; } = "Bearer";

        [JsonPropertyName("accessToken")]
        public string AccessToken { get; set; } = string.Empty;

        [JsonPropertyName("expiresIn")]
        public int ExpiresIn { get; set; }

        [JsonPropertyName("refreshToken")]
        public string RefreshToken { get; set; } = string.Empty;
    }
}