using System.Numerics;
using SkiaSharp;
using WaxMapArt.Comparison;
using WaxMapArt.Models;

namespace WaxMapArt.Dithering;

public class FloydSteinbergDithering(float errorDiffusionStrength = 1.0f, bool serpentineScanning = false) : IDithering
{
    public PixelMatch[,] Apply(SKBitmap image, Palette palette,
        IColorComparison comparison, StaircaseMode mode)
    {
        var entries = PaletteResolver.Resolve(palette, mode);
        var result = new PixelMatch[image.Width, image.Height];
        var errorBuf = new (float r, float g, float b)[image.Width, image.Height];

        for (var y = 0; y < image.Height; y++)
        {
            var isReversed = serpentineScanning && y % 2 == 1;
            var xStart = isReversed ? image.Width - 1 : 0;
            var xEnd = isReversed ? -1 : image.Width;
            var xStep = isReversed ? -1 : 1;

            for (var x = xStart; x != xEnd; x += xStep)
            {
                var pixel = image.GetPixel(x, y);
                var err = errorBuf[x, y];

                var adjusted = new SKColor(
                    (byte)Math.Clamp(pixel.Red + err.r, 0, 255),
                    (byte)Math.Clamp(pixel.Green + err.g, 0, 255),
                    (byte)Math.Clamp(pixel.Blue + err.b, 0, 255));

                var best = entries[0];
                var bestDistance = comparison.GetColorDifference(adjusted, best.Color);

                for (var i = 1; i < entries.Length; i++)
                {
                    var distance = comparison.GetColorDifference(adjusted, entries[i].Color);
                    if (distance < bestDistance)
                    {
                        bestDistance = distance;
                        best = entries[i];
                    }
                }

                result[x, y] = best.Match;

                var error = new Vector3(
                    adjusted.Red - best.Color.Red,
                    adjusted.Green - best.Color.Green,
                    adjusted.Blue - best.Color.Blue) * errorDiffusionStrength;

                DistributeError(errorBuf, x, y, error, xStep, image.Width, image.Height);
            }
        }

        return result;
    }

    private static void DistributeError((float r, float g, float b)[,] errors,
        int x, int y, Vector3 error, int direction, int width, int height)
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
            if (nx < 0 || nx >= width || ny < 0 || ny >= height) return;

            errors[nx, ny] = (
                errors[nx, ny].r + error.X * factor,
                errors[nx, ny].g + error.Y * factor,
                errors[nx, ny].b + error.Z * factor);
        }
    }
}
