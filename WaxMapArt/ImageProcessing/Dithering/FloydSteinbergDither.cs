using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace WaxMapArt.ImageProcessing.Dithering;

public static class FloydSteinbergDithering
{
    private static readonly int[,] Matrix =
    {
        { 0, 0, 7 },
        { 3, 5, 1 }
    };

    public static void ApplyDithering(ref Image<Rgb24> image)
    {
        int width = image.Width;
        int height = image.Height;

        for (int y = 0; y < height; y++)
        for (int x = 0; x < width; x++)
        {
            Rgb24 currentPixel = image[x, y];

            Rgb24 newPixel = QuantizePixel(currentPixel);
            Rgb24 error = currentPixel.Subtract(newPixel);

            int matrixIndex = y % 2 == 0 ? x % 2 : (x + 1) % 2;
            int matrixRow = y % 2 == 0 ? 0 : 1;

            if (x + 1 < width)
                image[x + 1, y] = image[x + 1, y]
                    .Sum(error.Multiply(Matrix[matrixRow, (matrixIndex + 1) % 3]).Divide(16));

            if (y + 1 < height)
                image[x, y + 1] = image[x, y + 1]
                    .Sum(error.Multiply(Matrix[(matrixRow + 1) % 2, matrixIndex]).Divide(16));

            if (x + 1 < width && y + 1 < height)
                image[x + 1, y + 1] = image[x + 1, y + 1]
                    .Sum(error.Multiply(Matrix[(matrixRow + 1) % 2, (matrixIndex + 1) % 3]).Divide(16));

            image[x, y] = newPixel;
        }
    }

    private static Rgb24 QuantizePixel(Rgb24 color)
    {
        byte r = (byte)(color.R / 24 * 24);
        byte g = (byte)(color.G / 24 * 24);
        byte b = (byte)(color.B / 24 * 24);

        return new Rgb24(r, g, b);
    }
}