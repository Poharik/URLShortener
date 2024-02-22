using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace URLShortener.API.Models.Requests;

public class RegisterRequest
{
    [Required]
    [JsonPropertyName("username")]
    public string Username { get; set; }

    [Required]
    [JsonPropertyName("password")]
    public string Password { get; set; }

    [Required]
    [JsonPropertyName("email")]
    public string Email { get; set; }
}