using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using WaxMapArt.ImageProcessing.Dithering;

namespace WaxMapArt.ImageProcessing;

public class ImageProcessor
{
    public WaxSize OutputSize;
    public DitheringType Dithering;

    public ImageProcessor(WaxSize outputSize, DitheringType dithering)
    {
        OutputSize = outputSize;
        Dithering = dithering;
    }

    public Image<Rgb24> Process(Image<Rgb24> image)
    {
        Image<Rgb24> output = image.Clone();

        output.Mutate(ctx => ctx.Resize(OutputSize.X, OutputSize.Y));

        switch (Dithering)
        {
            case DitheringType.FloydSteinberg:
                FloydSteinbergDithering.ApplyDithering(ref output);
                break;
        }

        return output;
    }
}