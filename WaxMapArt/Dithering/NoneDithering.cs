using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using WaxMapArt.Comparison;
using WaxMapArt.Utils;

namespace WaxMapArt.Dithering;

public class NoneDithering : IDithering
{
    public Image<Rgb24> ApplyDithering(Image<Rgb24> image, Palette palette, IColorComparison colorComparison)
    {
        var colors = palette.Colors.Select(color => ColorUtils.MapIdToInfo(color.MapId).Color).ToArray();
        
        for (var y = 0; y < image.Height; y++)
        for (var x = 0; x < image.Width; x++)
        {
            var originalColor = image[x, y];
            var nearestColor = ColorUtils.FindNearestColor(originalColor, colors, colorComparison);
            image[x, y] = nearestColor;
        }
        
        return image;
    }
}