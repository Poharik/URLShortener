using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using URLShortener.API.Models.Settings;
using URLShortener.API.Models.Requests;
namespace URLShortener.API.Controllers;

[ApiController]
[Route("user")]
public class UserController : ControllerBase
{
    private readonly JwtSettings _jwtSettings;

    public UserController(IOptions<JwtSettings> jwtSettings)
    {
        _jwtSettings = jwtSettings.Value;
    }

    [HttpPost("register")]
    [ProducesResponseType<string>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Register([FromBody]RegisterRequest registerRequest)
    {
        var token = GenerateToken(registerRequest.Username);
        return Ok(token);
    }

    [HttpPost("login")]
    [ProducesResponseType<string>(StatusCodes.Status200OK)]
    public async Task<IActionResult> LogIn([FromBody]LoginRequest loginRequest)
    {
        var token = GenerateToken(loginRequest.Username);
        return Ok(token);
    }

    private string GenerateToken(string username)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_jwtSettings.Key);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new Claim[] 
            {
                new("Username", username)
            }),
            IssuedAt = DateTime.UtcNow,
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience,
            Expires = DateTime.UtcNow.AddMinutes(30),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}