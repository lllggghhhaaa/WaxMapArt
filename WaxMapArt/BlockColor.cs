using SixLabors.ImageSharp.PixelFormats;

namespace WaxMapArt;

public record struct BlockColor(Rgb24 Color, BlockInfo Info);