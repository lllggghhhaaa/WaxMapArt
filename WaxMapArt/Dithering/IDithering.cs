using SkiaSharp;
using WaxMapArt.Comparison;
using WaxMapArt.Models;

namespace WaxMapArt.Dithering;

public enum StaircaseMode
{
    Flat,
    Staircase,
    AdaptiveStaircase
}

public interface IDithering
{
    PixelMatch[,] Apply(SKBitmap image, Palette palette,
        IColorComparison comparison, StaircaseMode mode);
}
