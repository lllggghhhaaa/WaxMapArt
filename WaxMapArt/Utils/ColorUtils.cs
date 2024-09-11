using SixLabors.ImageSharp.PixelFormats;
using WaxMapArt.Comparison;

namespace WaxMapArt.Utils;

public static class ColorUtils
{
    internal static Rgb24 FindNearestColor(Rgb24 originalColor, Rgb24[] palette, IColorComparison colorComparison)
    {
        Rgb24 nearestColor = palette.First();
        double shortestDistance = double.MaxValue;

        foreach (var color in palette)
        {
            double distance = colorComparison.GetColorDifference(originalColor, color);
            if (distance < shortestDistance)
            {
                shortestDistance = distance;
                nearestColor = color;
            }
        }

        return nearestColor;
    }
}