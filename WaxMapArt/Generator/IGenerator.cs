using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using WaxMapArt.Comparison;
using WaxMapArt.Entities;

namespace WaxMapArt.Generator;

public interface IGenerator
{
    public GeneratorOutput Generate(Image<Rgb24> image, Palette palette);
}

public record struct GeneratorOutput(BlockInfo[] Blocks);