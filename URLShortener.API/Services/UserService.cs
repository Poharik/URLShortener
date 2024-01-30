using System.Linq.Expressions;
using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using URLShortener.API.Models.Settings;
using URLShortener.API.Models.Database;

namespace URLShortener.API.Services;

public class UserService
{
    private readonly IMongoCollection<DbUser> _usersCollection;

    public UserService(IOptions<DatabaseSettings> databaseSettings)
    {
        var mongoClient = new MongoClient(databaseSettings.Value.ConnectionString);
        var mongoDatabase = mongoClient.GetDatabase(databaseSettings.Value.DatabaseName);
        _usersCollection = mongoDatabase.GetCollection<DbUser>(databaseSettings.Value.UsersCollectionName);
    }

    public async Task<IEnumerable<DbUser>> GetUsers(Expression<Func<DbUser, bool>> expression)
    {
        var result = await _usersCollection.FindAsync(expression);
        return result.ToEnumerable();
    }

    public async Task<DbUser?> GetUserByUsername(string username)
    {
        var foundUser = await GetUsers(x => x.Username == username);
        return foundUser.FirstOrDefault();
    }

    public async Task<DbUser?> GetUserByEmail(string email)
    {
        var foundUser = await GetUsers(x => x.Email == email);
        return foundUser.FirstOrDefault();
    }

    public Task AddUser(DbUser user) => _usersCollection.InsertOneAsync(user);
}