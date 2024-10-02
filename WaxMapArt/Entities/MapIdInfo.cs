using SixLabors.ImageSharp.PixelFormats;

namespace WaxMapArt.Entities;

public record struct MapIdInfo(int MapId, string Name, Rgb24 Color);