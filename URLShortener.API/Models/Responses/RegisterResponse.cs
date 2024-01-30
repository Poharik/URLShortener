using System.Text.Json.Serialization;

namespace URLShortener.API.Models.Responses;

public class RegisterResponse
{
    [JsonPropertyName("token")]
    public string Token { get; set; }

    [JsonPropertyName("expiresOn")]
    public DateTime ExpiresOn { get; set; }
}