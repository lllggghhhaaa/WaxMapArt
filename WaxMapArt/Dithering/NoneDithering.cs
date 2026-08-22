using SkiaSharp;
using WaxMapArt.Comparison;
using WaxMapArt.Models;

namespace WaxMapArt.Dithering;

public class NoneDithering : IDithering
{
    public PixelMatch[,] Apply(SKBitmap image, Palette palette,
        IColorComparison comparison, StaircaseMode mode)
    {
        var entries = PaletteResolver.Resolve(palette, mode);
        var result = new PixelMatch[image.Width, image.Height];

        for (var y = 0; y < image.Height; y++)
        for (var x = 0; x < image.Width; x++)
        {
            var pixel = image.GetPixel(x, y);
            result[x, y] = FindBestMatch(pixel, entries, comparison);
        }

        return result;
    }

    private static PixelMatch FindBestMatch(SKColor color, PaletteEntry[] entries,
        IColorComparison comparison)
    {
        var best = entries[0];
        var bestDistance = comparison.GetColorDifference(color, best.Color);

        for (var i = 1; i < entries.Length; i++)
        {
            var distance = comparison.GetColorDifference(color, entries[i].Color);
            if (distance < bestDistance)
            {
                bestDistance = distance;
                best = entries[i];
            }
        }

        return best.Match;
    }
}
