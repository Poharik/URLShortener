using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace URLShortener.API.Models.Database;

public class DbUser
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string ID { get; set; }

    [BsonElement("Username")]
    [BsonRepresentation(BsonType.String)]
    public string Username { get; set; }

    [BsonElement("Email")]
    [BsonRepresentation(BsonType.String)]
    public string Email { get; set; }

    [BsonElement("EmailVerified")]
    [BsonRepresentation(BsonType.Boolean)]
    public bool EmailVerified { get; set; } = false;

    [BsonElement("PasswordHash")]
    [BsonRepresentation(BsonType.String)]
    public string PasswordHash { get; set; }

    [BsonElement("DateCreated")]
    [BsonRepresentation(BsonType.DateTime)]
    public DateTime DateCreated { get; set; }
}