using SkiaSharp;
using WaxMapArt.Comparison;
using WaxMapArt.Utils;

namespace WaxMapArt.Dithering;

public class JarvisJudiceNinkeDithering(float errorDiffusionStrength = 1.0f, bool serpentineScanning = false) : IDithering
{
    public SKBitmap ApplyDithering(SKBitmap image, Palette palette, IColorComparison colorComparison, bool staircase = false)
    {
        var colors = ColorUtils.GetPaletteColors(palette, staircase);
        
        var errorImage = new float[image.Width, image.Height, 3];
        
        for (var y = 0; y < image.Height; y++)
        for (var x = 0; x < image.Width; x++)
        {
            var pixel = image.GetPixel(x, y);
            errorImage[x, y, 0] = pixel.Red;
            errorImage[x, y, 1] = pixel.Green;
            errorImage[x, y, 2] = pixel.Blue;
        }
        
        for (var y = 0; y < image.Height; y++)
        {
            var isReversed = serpentineScanning && y % 2 == 1;
            var xStart = isReversed ? image.Width - 1 : 0;
            var xEnd = isReversed ? -1 : image.Width;
            var xStep = isReversed ? -1 : 1;

            for (var x = xStart; x != xEnd; x += xStep)
            {
                var currentR = (byte)Math.Clamp(errorImage[x, y, 0], 0, 255);
                var currentG = (byte)Math.Clamp(errorImage[x, y, 1], 0, 255);
                var currentB = (byte)Math.Clamp(errorImage[x, y, 2], 0, 255);
                
                var currentColor = new SKColor(currentR, currentG, currentB);
                
                var nearestColor = ColorUtils.FindNearestColor(currentColor, colors, colorComparison);
                
                image.SetPixel(x, y, nearestColor);
                
                var errorR = (currentR - nearestColor.Red) * errorDiffusionStrength;
                var errorG = (currentG - nearestColor.Green) * errorDiffusionStrength;
                var errorB = (currentB - nearestColor.Blue) * errorDiffusionStrength;
                
                DistributeError(errorImage, x, y, errorR, errorG, errorB, image.Width, image.Height, xStep);
            }
        }
        
        return image;
    }
    
    private void DistributeError(float[,,] errorImage, int x, int y, float errorR, float errorG, float errorB, int width, int height, int direction)
    {
        var offsets = new (int dx, int dy, float weight)[]
        {
            (direction, 0, 7.0f/48.0f),
            (2 * direction, 0, 5.0f/48.0f),
            (-2 * direction, 1, 3.0f/48.0f),
            (-direction, 1, 5.0f/48.0f),
            (0, 1, 7.0f/48.0f),
            (direction, 1, 5.0f/48.0f),
            (2 * direction, 1, 3.0f/48.0f),
            (-2 * direction, 2, 1.0f/48.0f),
            (-direction, 2, 3.0f/48.0f),
            (0, 2, 5.0f/48.0f),
            (direction, 2, 3.0f/48.0f),
            (2 * direction, 2, 1.0f/48.0f)
        };
        
        foreach (var (dx, dy, weight) in offsets)
        {
            var nx = x + dx;
            var ny = y + dy;

            if (nx < 0 || nx >= width || ny < 0 || ny >= height) continue;
            errorImage[nx, ny, 0] += errorR * weight;
            errorImage[nx, ny, 1] += errorG * weight;
            errorImage[nx, ny, 2] += errorB * weight;
        }
    }
}