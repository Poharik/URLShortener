using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Microsoft.AspNetCore.Identity;
using URLShortener.API.Models.Settings;
using URLShortener.API.Models.Database;
using URLShortener.API.Models.Requests;
using URLShortener.API.Models.Results;

namespace URLShortener.API.Services;

public class UserService
{
    private const string InvalidCredentialsMessage = "Invalid username or password.";

    private readonly IMongoCollection<UserRecord> _usersCollection;

    public UserService(IOptions<DatabaseSettings> databaseSettings)
    {
        var mongoClient = new MongoClient(databaseSettings.Value.ConnectionString);
        var mongoDatabase = mongoClient.GetDatabase(databaseSettings.Value.DatabaseName);
        _usersCollection = mongoDatabase.GetCollection<UserRecord>(databaseSettings.Value.UserCollectionName);
    }

    public async Task<RegisterResult> RegisterAsync(RegisterRequest registerRequest)
    {
        // check if the email and username are available
        var conflictedUser = await _usersCollection.FindAsync(x => x.Username == registerRequest.Username);
        if (conflictedUser.Any())
        {
            return new RegisterResult
            {
                Success = false,
                Message = $"Username '{registerRequest.Username}' already in use."
            };
        }
        
        conflictedUser = await _usersCollection.FindAsync(x => x.Email == registerRequest.Email);
        if (conflictedUser.Any())
        {
            return new RegisterResult
            {
                Success = false,
                Message = $"Email '{registerRequest.Email}' already in use."
            };
        }
        
        // hash the password
        var passwordHasher = new PasswordHasher<object>();
        var passwordHash = passwordHasher.HashPassword(null!, registerRequest.Password);

        // add user to database
        await _usersCollection.InsertOneAsync(new UserRecord
        {
            Username = registerRequest.Username,
            Email = registerRequest.Email,
            PasswordHash = passwordHash,
            DateCreated = DateTime.Now,
        });

        return new RegisterResult { Success = true };
    }

    public async Task<LogInResult> LogInAsync(LogInRequest loginRequest)
    {
        var foundRecords = await _usersCollection.FindAsync(x => x.Username == loginRequest.Username);
        var foundUser = foundRecords.FirstOrDefault();

        if (foundUser == default)
        {
            return new LogInResult
            {
                Success = false,
                Message = InvalidCredentialsMessage
            };
        }

        var passwordHash = foundUser.PasswordHash;
        var passwordHasher = new PasswordHasher<object>();
        var veritifcationResult = passwordHasher.VerifyHashedPassword(null!, passwordHash, loginRequest.Password);
        if (veritifcationResult == PasswordVerificationResult.Failed)
        {
            return new LogInResult
            {
                Success = false,
                Message = InvalidCredentialsMessage
            };
        }

        return new LogInResult { Success = true };
    }
}