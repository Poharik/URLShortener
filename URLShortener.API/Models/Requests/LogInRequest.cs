using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace URLShortener.API.Models.Requests;

public class LogInRequest
{
    [Required]
    [JsonPropertyName("username")]
    public string Username { get; set; }
    
    [Required]
    [JsonPropertyName("password")]
    public string Password { get; set; }
}