using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Newtonsoft.Json;

namespace WaxMapArt.Bot.Models;

public class User
{
    [JsonProperty("user_id")]
    public string UserId = String.Empty;
    [JsonProperty("palettes")]
    public List<Palette> Palettes = new();

    public static async Task<User> GetFromDatabaseAsync(IMongoDatabase database, ulong id)
    {
        var collection = database.GetCollection<BsonDocument>("Users");
        var filter = new BsonDocument { { "user_id", id.ToString() } };
        
        if (await collection.CountDocumentsAsync(filter) <= 0) 
            return new User
            {
                UserId = id.ToString(),
                Palettes = new List<Palette>
                {
                    JsonConvert.DeserializeObject<Palette>(await File.ReadAllTextAsync("palette.json"))
                }
            };

        BsonDocument? doc = (await collection.FindAsync(filter)).First();
        doc.Remove("_id");
        
        return JsonConvert.DeserializeObject<User>(doc.ToJson())!;
    }

    public async Task SendToDatabaseAsync(IMongoDatabase database)
    {
        var collection = database.GetCollection<BsonDocument>("Users");
        var filter = new BsonDocument { { "user_id", UserId } };
        
        string json = JsonConvert.SerializeObject(this);
        var document = BsonSerializer.Deserialize<BsonDocument>(json);
        
        if (await collection.CountDocumentsAsync(filter) > 0)
            await collection.ReplaceOneAsync(filter, document);
        else
            await collection.InsertOneAsync(document);
    }
}