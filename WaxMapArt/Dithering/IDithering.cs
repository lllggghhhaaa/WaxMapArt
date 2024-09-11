using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace WaxMapArt.Dithering;

public interface IDithering
{ 
    public Image<Rgb24> ApplyDithering(Image<Rgb24> image, Palette palette);
}