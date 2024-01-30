using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using URLShortener.API.Models.Settings;
using URLShortener.API.Models.Requests;
using URLShortener.API.Models.Responses;

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
    [ProducesResponseType<AuthResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Register([FromBody]RegisterRequest registerRequest)
    {
        var expires = DateTime.UtcNow.AddMinutes(30);
        var token = GenerateToken(registerRequest.Username, expires);
        
        return Ok(new AuthResponse
        {
            Token = token,
            ExpiresOn = expires
        });
    }

    [HttpPost("login")]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> LogIn([FromBody]LoginRequest loginRequest)
    {
        var expires = DateTime.UtcNow.AddMinutes(30);
        var token = GenerateToken(loginRequest.Username, expires);

        return Ok(new AuthResponse
        {
            Token = token,
            ExpiresOn = expires
        });
    }

    private string GenerateToken(string username, DateTime expires)
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
            Expires = expires,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}