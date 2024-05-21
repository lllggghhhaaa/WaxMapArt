using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using WaxMapArt.ImageProcessing.Dithering;

namespace WaxMapArt.ImageProcessing;

public class ImageProcessor(WaxSize outputSize)
{
    public Image<Rgb24> Process(Image<Rgb24> image)
    {
        Image<Rgb24> output = image.Clone();

        output.Mutate(ctx => ctx.Resize(outputSize.X, outputSize.Y));

        return output;
    }

    public static void ApplyDither(ref Image<Rgb24> pImage, DitheringType ditheringType, List<WaxColor> ditherPalette, ComparisonMethod method)
    {
        IWaxDithering dithering = ditheringType switch
        {
            DitheringType.None => new NoDithering(),
            DitheringType.FloydSteinberg => new FloydSteinbergDithering(),
            DitheringType.BayerOrdered4X4 => new BayerOrderedDithering(BayerOrderedDithering.Bayer4X4),
            DitheringType.BayerOrdered8X8 => new BayerOrderedDithering(BayerOrderedDithering.Bayer8X8),
            DitheringType.BayerOrdered16X16 => new BayerOrderedDithering(BayerOrderedDithering.Bayer16X16),
            _ => throw new ArgumentOutOfRangeException()
        };
        
        dithering.ApplyDither(ref pImage, ditherPalette, method);
    }
}