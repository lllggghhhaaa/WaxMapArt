using Newtonsoft.Json;
using SixLabors.ImageSharp.PixelFormats;

namespace WaxMapArt;

public struct BlockInfo
{
    [JsonProperty("id")] public string BlockId;
    [JsonProperty("map_id")] public int MapId;
    [JsonProperty("properties")] public Dictionary<string, string> Properties;
    
    public Rgb24 GetBaseColor() => MapColors.BaseColors[MapId];
}