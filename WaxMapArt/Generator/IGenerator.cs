using SkiaSharp;
using WaxMapArt.Entities;

namespace WaxMapArt.Generator;

public interface IGenerator
{
    public GeneratorOutput Generate(SKBitmap image, Palette palette);
}

public record struct GeneratorOutput(BlockInfo[] Blocks);