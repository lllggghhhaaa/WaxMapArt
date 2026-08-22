using SkiaSharp;
using WaxMapArt.Models;

namespace WaxMapArt.Generators;

public class GeneratorOutput
{
    public IReadOnlyList<BlockInfo> Blocks { get; }

    public GeneratorOutput(IReadOnlyList<BlockInfo> blocks)
    {
        Blocks = blocks;
    }

    public IReadOnlyDictionary<string, int> CountBlocks()
    {
        var counts = new Dictionary<string, int>();
        foreach (var block in Blocks)
        {
            counts.TryGetValue(block.Id, out var count);
            counts[block.Id] = count + 1;
        }
        return counts;
    }

    public static SKBitmap RenderPreview(PixelMatch[,] pixels, Palette palette)
    {
        var width = pixels.GetLength(0);
        var height = pixels.GetLength(1);
        var preview = new SKBitmap(width, height);

        var blockLookup = new Dictionary<string, (int ColorId, bool NeedSupport)>();
        foreach (var block in palette.Blocks)
            blockLookup[block.Id] = (block.ColorId, block.NeedSupport);

        var shadingFactors = new[] { 0.71, 0.86, 1.0 };

        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            var match = pixels[x, y];
            if (!blockLookup.TryGetValue(match.Id, out var info))
            {
                preview.SetPixel(x, y, SKColors.Black);
                continue;
            }

            var shade = match.Shading < shadingFactors.Length ? shadingFactors[match.Shading] : 1.0;
            var baseColor = MapColors.GetColor(info.ColorId);
            preview.SetPixel(x, y, new SKColor(
                (byte)(baseColor.Red * shade),
                (byte)(baseColor.Green * shade),
                (byte)(baseColor.Blue * shade)));
        }

        return preview;
    }
}
