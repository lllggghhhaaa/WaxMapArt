using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace WaxMapArt.ImageProcessing.Dithering;

public interface IWaxDithering
{
    public void ApplyDither(ref Image<Rgb24> image); 
}