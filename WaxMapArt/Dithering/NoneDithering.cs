using SkiaSharp;
using WaxMapArt.Comparison;
using WaxMapArt.Utils;

namespace WaxMapArt.Dithering;

public class NoneDithering : IDithering
{
    public SKBitmap ApplyDithering(SKBitmap image, Palette palette, IColorComparison colorComparison, bool staircase)
    {
        var colors = ColorUtils.GetPaletteColors(palette, staircase);
        
        for (var y = 0; y < image.Height; y++)
        for (var x = 0; x < image.Width; x++)
        {
            var originalColor = image.GetPixel(x, y);
            var nearestColor = ColorUtils.FindNearestColor(originalColor, colors, colorComparison);
            image.SetPixel(x, y, nearestColor);
        }
        
        return image;
    }
}