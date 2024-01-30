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
using URLShortener.API.Models.Database;
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
    public async Task<IActionResult> Register([FromBody]RegisterRequest registerRequest)
    {
        // check if the email and username are available
        var conflictedUser = await _userService.GetUserByUsername(registerRequest.Username);
        if (conflictedUser is not null)
            return Unauthorized($"Username '{registerRequest.Username}' already in use.");

        conflictedUser = await _userService.GetUserByEmail(registerRequest.Email);
        if (conflictedUser is not null)
            return Unauthorized($"Email '{registerRequest.Email}' already in use.");

        // hash the password
        var passwordHasher = new PasswordHasher<object>();
        var passwordHash = passwordHasher.HashPassword(null!, registerRequest.Password);
        
        // add user to database
        await _userService.AddUser(new DbUser
        {
            Username = registerRequest.Username,
            Email = registerRequest.Email,
            PasswordHash = passwordHash,
            DateCreated = DateTime.Now,
        });

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
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> LogIn([FromBody]LoginRequest loginRequest)
    {
        // check if user exists
        var foundUser = await _userService.GetUserByUsername(loginRequest.Username);
        if (foundUser is null)
            return Unauthorized("Invalid credentials.");
        
        // validate password
        var passwordHasher = new PasswordHasher<object>();
        var veritifcationResult = passwordHasher.VerifyHashedPassword(null!, foundUser.PasswordHash, loginRequest.Password);
        if (veritifcationResult == PasswordVerificationResult.Failed)
            return Unauthorized("Invalid credentials.");

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