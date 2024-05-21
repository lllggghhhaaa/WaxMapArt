using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace WaxMapArt.ImageProcessing.Dithering;

public class FloydSteinbergDithering : IWaxDithering
{
    public void ApplyDither(ref Image<Rgb24> image, List<WaxColor> palette, ComparisonMethod comparisonMethod)
    {
        int width = image.Width;
        int height = image.Height;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                WaxColor oldColor = WaxColor.FromRgb24(image[x, y]);
                WaxColor closestColor = oldColor.Nearest(palette, comparisonMethod);

                image[x, y] = closestColor.ToRgb24();

                int errorR = oldColor.R - closestColor.R;
                int errorG = oldColor.G - closestColor.G;
                int errorB = oldColor.B - closestColor.B;

                PropagateError(image, x, y, errorR, errorG, errorB);
            }
        }
    }
    
    private void PropagateError(Image<Rgb24> image, int x, int y, int errorR, int errorG, int errorB)
    {
        int[] errorPropagation = [7, 3, 5, 1];

        for (int i = 0; i < 4; i++)
        {
            int offsetX = i - 1;
            int offsetY = 1;

            if (x + offsetX >= 0 && x + offsetX < image.Width &&
                y + offsetY >= 0 && y + offsetY < image.Height)
            {
                Rgb24 currentColor = image[x + offsetX, y + offsetY];

                int newR = Clamp(currentColor.R + (int)(errorR * (errorPropagation[i] / 16d)));
                int newG = Clamp(currentColor.G + (int)(errorG * (errorPropagation[i] / 16d)));
                int newB = Clamp(currentColor.B + (int)(errorB * (errorPropagation[i] / 16d)));

                image[x + offsetX, y + offsetY] = new Rgb24((byte)newR, (byte)newG, (byte)newB);
            }
        }
    }
    
    private int Clamp(int value) => Math.Min(Math.Max(value, 0), 255);
}