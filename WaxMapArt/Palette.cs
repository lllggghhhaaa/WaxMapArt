using Newtonsoft.Json;

namespace WaxMapArt;

public struct Palette
{
    [JsonProperty("colors")]
    public Dictionary<int, BlockInfo> Colors;
    [JsonProperty("placeholder_block")]
    public BlockInfo PlaceholderBlock;
}