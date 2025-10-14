using SkiaSharp;

namespace WaxMapArt.Entities;

public struct MapIdInfo(int mapId, string name, SKColor color)
{
    public int MapId = mapId;
    public byte Shading = 2;
    public string Name = name;
    public SKColor Color = color;
    public Dictionary<string, string> Properties = new();

    public MapIdInfo Clone() => new(MapId, Name, Color) { Properties = Properties };
}