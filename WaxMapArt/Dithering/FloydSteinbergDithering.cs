using System.Numerics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using WaxMapArt.Comparison;
using WaxMapArt.Utils;

namespace WaxMapArt.Dithering;

public class FloydSteinbergDithering : IDithering
{
    public Image<Rgb24> ApplyDithering(Image<Rgb24> image, Palette palette, IColorComparison colorComparison, bool staircase)
    {
        var colors = ColorUtils.GetPaletteColors(palette, staircase);
        
        for (var y = 0; y < image.Height; y++)
        for (var x = 0; x < image.Width; x++)
        {
            var originalColor = image[x, y];
            var nearestColor = ColorUtils.FindNearestColor(originalColor, colors, colorComparison);
            image[x, y] = nearestColor;

            var error = new Vector3(
                originalColor.R - nearestColor.R,
                originalColor.G - nearestColor.G,
                originalColor.B - nearestColor.B
            );

            DistributeError(image, x, y, error);
        }

        return image;
    }

    private static void DistributeError(Image<Rgb24> image, int x, int y, Vector3 error)
    {
        // Floyd-Steinberg dithering coefficients
        AddError(1, 0, 7.0f / 16.0f);
        AddError(-1, 1, 3.0f / 16.0f);
        AddError(0, 1, 5.0f / 16.0f);
        AddError(1, 1, 1.0f / 16.0f);
        return;

        void AddError(int offsetX, int offsetY, float factor)
        {
            var nx = x + offsetX;
            var ny = y + offsetY;
            
            if (nx < 0 || nx >= image.Width || ny < 0 || ny >= image.Height) return;
            
            var pixel = image[nx, ny];
            var newColor = new Vector3(pixel.R, pixel.G, pixel.B) + error * factor;
            image[nx, ny] = new Rgb24(
                (byte)Math.Clamp(newColor.X, 0, 255),
                (byte)Math.Clamp(newColor.Y, 0, 255),
                (byte)Math.Clamp(newColor.Z, 0, 255)
            );
        }
    }
}