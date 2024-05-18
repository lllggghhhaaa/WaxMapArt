using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace WaxMapArt.ImageProcessing.Dithering;

public class FloydSteinbergDithering : IWaxDithering
{
    public void ApplyDither(ref Image<Rgb24> image)
    {
        throw new NotImplementedException();
    }
}