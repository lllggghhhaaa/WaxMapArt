using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using WaxMapArt.Comparison;

namespace WaxMapArt.Dithering;

public interface IDithering
{ 
    public Image<Rgb24> ApplyDithering(Image<Rgb24> image, Palette palette, IColorComparison colorComparison);
}

public enum DitheringMode
{
    Atkinson,
    FloydSteinberg,
}