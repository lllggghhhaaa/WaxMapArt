using MongoDB.Driver;

namespace WaxMapArt.Bot.Models;

public class User
{
    public string UserId = String.Empty;
    public List<Palette> Palettes = new();
    
    public static async Task<User?> GetFromDatabaseAsync(IMongoDatabase database, ulong id)
    {
        IMongoCollection<User> collection = database.GetCollection<User>("Users");
        
        if (await collection.CountDocumentsAsync(user => user.UserId == id.ToString()) <= 0) return null;

        return (await collection.FindAsync(user => user.UserId == id.ToString())).First();
    }

    public async Task SendToDatabaseAsync(IMongoDatabase database)
    {
        IMongoCollection<User> collection = database.GetCollection<User>("Users");
        
        await collection.InsertOneAsync(this);
    }
}