using System.Text.Json.Serialization;

namespace URLShortener.API.Models.Requests;

public class RegisterRequest
{
    [JsonPropertyName("username")]
    public string Username { get; set; }

    [JsonPropertyName("password")]
    public string Password { get; set; }

    [JsonPropertyName("email")]
    public string Email { get; set; }
}