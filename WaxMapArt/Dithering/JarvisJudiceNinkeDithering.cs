using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using WaxMapArt.Comparison;
using WaxMapArt.Utils;

namespace WaxMapArt.Dithering;

public class JarvisJudiceNinkeDithering : IDithering
{
    public Image<Rgb24> ApplyDithering(Image<Rgb24> image, Palette palette, IColorComparison colorComparison, bool staircase = false)
    {
        var colors = ColorUtils.GetPaletteColors(palette, staircase);
        
        var errorImage = new float[image.Width, image.Height, 3];
        
        for (var y = 0; y < image.Height; y++)
        for (var x = 0; x < image.Width; x++)
        {
            var pixel = image[x, y];
            errorImage[x, y, 0] = pixel.R;
            errorImage[x, y, 1] = pixel.G;
            errorImage[x, y, 2] = pixel.B;
        }
        
        for (var y = 0; y < image.Height; y++)
        for (var x = 0; x < image.Width; x++)
        {
            var currentR = (byte)Math.Clamp(errorImage[x, y, 0], 0, 255);
            var currentG = (byte)Math.Clamp(errorImage[x, y, 1], 0, 255);
            var currentB = (byte)Math.Clamp(errorImage[x, y, 2], 0, 255);
            
            var currentColor = new Rgb24(currentR, currentG, currentB);
            
            var nearestColor = ColorUtils.FindNearestColor(currentColor, colors, colorComparison);
            
            image[x, y] = nearestColor;
            
            var errorR = currentR - nearestColor.R;
            var errorG = currentG - nearestColor.G;
            var errorB = currentB - nearestColor.B;
            
            DistributeError(errorImage, x, y, errorR, errorG, errorB, image.Width, image.Height);
        }
        
        return image;
    }
    
    private void DistributeError(float[,,] errorImage, int x, int y, float errorR, float errorG, float errorB, int width, int height)
    {
        var offsets = new (int dx, int dy, float weight)[]
        {
            (1, 0, 7.0f/48.0f),
            (2, 0, 5.0f/48.0f),
            (-2, 1, 3.0f/48.0f),
            (-1, 1, 5.0f/48.0f),
            (0, 1, 7.0f/48.0f),
            (1, 1, 5.0f/48.0f),
            (2, 1, 3.0f/48.0f),
            (-2, 2, 1.0f/48.0f),
            (-1, 2, 3.0f/48.0f),
            (0, 2, 5.0f/48.0f),
            (1, 2, 3.0f/48.0f),
            (2, 2, 1.0f/48.0f)
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