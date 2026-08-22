using SkiaSharp;
using WaxMapArt.Comparison;
using WaxMapArt.Models;

namespace WaxMapArt.Dithering;

public class JarvisJudiceNinkeDithering : IDithering
{
    public PixelMatch[,] Apply(SKBitmap image, Palette palette,
        IColorComparison comparison, StaircaseMode mode)
    {
        var entries = PaletteResolver.Resolve(palette, mode);
        var result = new PixelMatch[image.Width, image.Height];
        var errorBuf = new (float r, float g, float b)[image.Width, image.Height];

        for (var y = 0; y < image.Height; y++)
        for (var x = 0; x < image.Width; x++)
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

            var quantErrorR = adjusted.Red - best.Color.Red;
            var quantErrorG = adjusted.Green - best.Color.Green;
            var quantErrorB = adjusted.Blue - best.Color.Blue;

            var factor = 1.0f / 48.0f;
            AddError(1, 0, 7 * factor);
            AddError(2, 0, 5 * factor);
            AddError(-2, 1, 3 * factor);
            AddError(-1, 1, 5 * factor);
            AddError(0, 1, 7 * factor);
            AddError(1, 1, 5 * factor);
            AddError(2, 1, 3 * factor);
            AddError(-2, 2, 1 * factor);
            AddError(-1, 2, 3 * factor);
            AddError(0, 2, 5 * factor);
            AddError(1, 2, 3 * factor);
            AddError(2, 2, 1 * factor);
            continue;

            void AddError(int offsetX, int offsetY, float f)
            {
                var nx = x + offsetX;
                var ny = y + offsetY;
                if (nx < 0 || nx >= image.Width || ny < 0 || ny >= image.Height) return;
                errorBuf[nx, ny] = (
                    errorBuf[nx, ny].r + quantErrorR * f,
                    errorBuf[nx, ny].g + quantErrorG * f,
                    errorBuf[nx, ny].b + quantErrorB * f);
            }
        }

        return result;
    }
}
