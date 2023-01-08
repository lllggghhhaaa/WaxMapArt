using Newtonsoft.Json;

namespace WaxMapArt;

public struct Palette
{
    [JsonProperty("name")] public string Name { get; set; }
    [JsonProperty("colors")] public Dictionary<string, BlockInfo> Colors { get; set; }
    [JsonProperty("placeholder_block")] public BlockInfo PlaceholderBlock { get; set; }
}