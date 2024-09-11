using Newtonsoft.Json;

namespace WaxMapArt;

public struct Palette
{
    [JsonProperty("name")] public string Name;
    [JsonProperty("colors")] public PaletteColor[] Colors;
    [JsonProperty("placeholderColor")] public PaletteColor PlaceholderColor;
}

public struct PaletteColor
{
    [JsonProperty("id")] public string Id;
    [JsonProperty("map_id")] public int MapId;
    [JsonProperty("color")] public int Color; // hexadecimal
    [JsonProperty("properties")] public Dictionary<string, string> Properties { get; set; }
    [JsonProperty("generator_properties")] public GeneratorProperties GeneratorProperties { get; set; }
}

public struct GeneratorProperties
{
    [JsonProperty("need_support")] public bool NeedSupport;
}