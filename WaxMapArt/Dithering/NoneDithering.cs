using SkiaSharp;
using WaxMapArt.Comparison;
using WaxMapArt.Utils;

namespace WaxMapArt.Dithering;

public class NoneDithering : IDithering
{
    public SKBitmap ApplyDithering(SKBitmap image, Palette palette, IColorComparison colorComparison, StaircaseMode staircaseMode, double threshold)
    {
        var result = image.Copy();
        var colors = ColorUtils.GetPaletteColors(palette, staircaseMode is StaircaseMode.Staircase or StaircaseMode.AdaptiveStaircase);
        var flatColors = ColorUtils.GetPaletteColors(palette);
        
        for (var y = 0; y < result.Height; y++)
        for (var x = 0; x < result.Width; x++)
        {
            var originalColor = result.GetPixel(x, y);
            var nearestColor =
                ColorUtils.FindNearestColor(originalColor, colors, flatColors, colorComparison, staircaseMode, threshold);
            result.SetPixel(x, y, nearestColor);
        }

        return result;
    }
}