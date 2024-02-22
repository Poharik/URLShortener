using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using URLShortener.API.Services;
using URLShortener.API.Models.Settings;
using URLShortener.API.Models.Requests;
using URLShortener.API.Models.Responses;

namespace URLShortener.API.Controllers;

[ApiController]
[Route("user")]
public class UserController : ControllerBase
{
    private readonly JwtSettings _jwtSettings;
    private readonly UserService _userService;

    public UserController(IOptions<JwtSettings> jwtSettings, UserService userService)
    {
        _jwtSettings = jwtSettings.Value;
        _userService = userService;
    }

    [HttpPost("register")]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest registerRequest)
    {
        var registerResult = await _userService.Register(registerRequest);

        if (!registerResult.Succeeded)
            return Unauthorized(registerResult.Message);

        var expires = DateTime.UtcNow.Add(_jwtSettings.TokenLifetime);
        var jwtToken = GenerateToken(registerRequest.Username, expires);
        
        return Ok(new AuthResponse
        {
            Token = jwtToken,
            ExpiresOn = expires
        });
    }

    [HttpPost("login")]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> LogIn([FromBody] LogInRequest loginRequest)
    {
        var logInResult = await _userService.LogIn(loginRequest);

        if (!logInResult.Succeeded)
            return Unauthorized(logInResult.Message);

        var expires = DateTime.UtcNow.Add(_jwtSettings.TokenLifetime);
        var jwtToken = GenerateToken(loginRequest.Username, expires);

        return Ok(new AuthResponse
        {
            Token = jwtToken,
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