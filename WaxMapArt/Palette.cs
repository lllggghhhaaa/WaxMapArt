using Newtonsoft.Json;

namespace WaxMapArt;

public struct Palette
{
    [JsonProperty("name")] public string Name;
    [JsonProperty("colors")] public Dictionary<string, BlockInfo> Colors;
    [JsonProperty("placeholder_block")] public BlockInfo PlaceholderBlock;
}