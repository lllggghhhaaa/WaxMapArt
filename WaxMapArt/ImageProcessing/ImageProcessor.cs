using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace WaxMapArt.ImageProcessing;

public class ImageProcessor(WaxSize outputSize)
{
    public Image<Rgb24> Process(Image<Rgb24> image)
    {
        Image<Rgb24> output = image.Clone();

        output.Mutate(ctx => ctx.Resize(outputSize.X, outputSize.Y));

        return output;
    }
}