using System.Text.Json.Serialization;

namespace URLShortener.API.Models.Requests;

public class LoginRequest
{
    [JsonPropertyName("username")]
    public string Username { get; set; }
    
    [JsonPropertyName("password")]
    public string Password { get; set; }
}