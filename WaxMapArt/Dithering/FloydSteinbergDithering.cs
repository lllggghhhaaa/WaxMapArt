using System.Numerics;
using SkiaSharp;
using WaxMapArt.Comparison;
using WaxMapArt.Utils;

namespace WaxMapArt.Dithering;

public class FloydSteinbergDithering(float errorDiffusionStrength = 1.0f, bool serpentineScanning = false) : IDithering
{
    public SKBitmap ApplyDithering(SKBitmap image, Palette palette, IColorComparison colorComparison, bool staircase)
    {
        var colors = ColorUtils.GetPaletteColors(palette, staircase);
        
        for (var y = 0; y < image.Height; y++)
        {
            var isReversed = serpentineScanning && y % 2 == 1;
            var xStart = isReversed ? image.Width - 1 : 0;
            var xEnd = isReversed ? -1 : image.Width;
            var xStep = isReversed ? -1 : 1;

            for (var x = xStart; x != xEnd; x += xStep)
            {
                var originalColor = image.GetPixel(x, y);
                var nearestColor = ColorUtils.FindNearestColor(originalColor, colors, colorComparison);
                image.SetPixel(x, y, nearestColor);

                var error = new Vector3(
                    originalColor.Red - nearestColor.Red,
                    originalColor.Green - nearestColor.Green,
                    originalColor.Blue - nearestColor.Blue
                ) * errorDiffusionStrength;

                DistributeError(image, x, y, error, xStep);
            }
        }

        return image;
    }

    private static void DistributeError(SKBitmap image, int x, int y, Vector3 error, int direction)
    {
        AddError(direction, 0, 7.0f / 16.0f);
        AddError(-direction, 1, 3.0f / 16.0f);
        AddError(0, 1, 5.0f / 16.0f);
        AddError(direction, 1, 1.0f / 16.0f);
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