using Newtonsoft.Json;

namespace WaxMapArt;

public struct BlockInfo
{
    [JsonProperty("id")] public string BlockId { get; set; }
    [JsonProperty("map_id")] public int MapId { get; set; }
    [JsonProperty("color")] public WaxColor Color { get; set; }
    [JsonProperty("properties")] public Dictionary<string, string> Properties { get; set; }
    [JsonProperty("generator_properties")] public GeneratorProperties GeneratorProperties { get; set; }
}