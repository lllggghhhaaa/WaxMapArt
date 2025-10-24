using System.Numerics;
using SkiaSharp;
using WaxMapArt.Comparison;
using WaxMapArt.Utils;

namespace WaxMapArt.Dithering;

public class AtkinsonDithering(float errorDiffusionStrength = 1.0f, bool serpentineScanning = false) : IDithering
{
    public SKBitmap ApplyDithering(SKBitmap image, Palette palette, IColorComparison colorComparison, StaircaseMode staircaseMode, double threshold)
    {
        var result = image.Copy();
        var colors = ColorUtils.GetPaletteColors(palette, staircaseMode is StaircaseMode.Staircase or StaircaseMode.AdaptiveStaircase );
        var flatColors = ColorUtils.GetPaletteColors(palette);
        
        for (var y = 0; y < result.Height; y++)
        {
            var isReversed = serpentineScanning && y % 2 == 1;
            var xStart = isReversed ? result.Width - 1 : 0;
            var xEnd = isReversed ? -1 : result.Width;
            var xStep = isReversed ? -1 : 1;

            for (var x = xStart; x != xEnd; x += xStep)
            {
                var originalColor = result.GetPixel(x, y);
                var nearestColor =
                    ColorUtils.FindNearestColor(originalColor, colors, flatColors, colorComparison, staircaseMode, threshold);
                result.SetPixel(x, y, nearestColor);

                var error = new Vector3(
                    originalColor.Red - nearestColor.Red,
                    originalColor.Green - nearestColor.Green,
                    originalColor.Blue - nearestColor.Blue
                ) * errorDiffusionStrength;

                DistributeError(result, x, y, error, xStep);
            }
        }

        return result;
    }

    private static void DistributeError(SKBitmap image, int x, int y, Vector3 error, int direction)
    {
        AddError(direction, 0, 1.0f / 8.0f);
        AddError(2 * direction, 0, 1.0f / 8.0f);
        AddError(-direction, 1, 1.0f / 8.0f);
        AddError(0, 1, 1.0f / 8.0f);
        AddError(direction, 1, 1.0f / 8.0f);
        AddError(0, 2, 1.0f / 8.0f);
        return;

        void AddError(int offsetX, int offsetY, float factor)
        {
            var nx = x + offsetX;
            var ny = y + offsetY;
            
            if (nx < 0 || nx >= image.Width || ny < 0 || ny >= image.Height) return;
            
            var pixel = image.GetPixel(nx, ny);
            var newColor = new Vector3(pixel.Red, pixel.Green, pixel.Blue) + error * factor;
            image.SetPixel(nx, ny, new SKColor(
                (byte)Math.Clamp(newColor.X, 0, 255),
                (byte)Math.Clamp(newColor.Y, 0, 255),
                (byte)Math.Clamp(newColor.Z, 0, 255)
            ));
        }
    }
}