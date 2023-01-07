using Newtonsoft.Json;

namespace WaxMapArt.Bot;

public struct ConfigJson
{
    [JsonProperty("token")] public string Token;
    [JsonProperty("mongo_uri")] public string MongoUri;
}