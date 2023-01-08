using Newtonsoft.Json;

namespace WaxMapArt;

public struct BlockInfo
{
    [JsonProperty("id")] public string BlockId { get; set; }
    [JsonProperty("map_id")] public int MapId { get; set; }
    [JsonProperty("properties")] public Dictionary<string, string> Properties { get; set; }
}