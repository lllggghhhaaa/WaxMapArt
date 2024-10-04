using SixLabors.ImageSharp.PixelFormats;

namespace WaxMapArt.Entities;

public struct MapIdInfo(int mapId, string name, Rgb24 color)
{
    public int MapId = mapId;
    public byte Shading = 2;
    public string Name = name;
    public Rgb24 Color = color;
    public Dictionary<string, string> Properties = new();

    public MapIdInfo Clone() => new(MapId, Name, Color) { Properties = Properties };
}