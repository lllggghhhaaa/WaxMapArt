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

    public void Process(ref Image<Rgb24> image)
    {
        image.Mutate(ctx => ctx.Resize(OutputSize.X, OutputSize.Y));

        switch (Dithering)
        {
            case DitheringType.FloydSteinberg:
                FloydSteinbergDithering.ApplyDithering(ref image);
                break;
        }
    }
}