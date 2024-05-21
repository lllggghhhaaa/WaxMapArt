using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace WaxMapArt.ImageProcessing.Dithering;

public class NoDithering : IWaxDithering
{
    public void ApplyDither(ref Image<Rgb24> image, List<WaxColor> palette, ComparisonMethod comparisonMethod)
    {
        for(int x = 0; x < image.Width; x++)
        for (int y = 0; y < image.Height; y++)
        {
            var color = WaxColor.FromRgb24(image[x, y]);
            image[x, y] = color.Nearest(palette, comparisonMethod).ToRgb24();
        }
    }
}