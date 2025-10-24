using SkiaSharp;
using WaxMapArt.Entities;

namespace WaxMapArt.Generator;

public interface IGenerator
{
    public GeneratorOutput Generate(SKBitmap ditheredImage, SKBitmap originalImage, Palette palette);}

public record struct GeneratorOutput(BlockInfo[] Blocks);