using SixLabors.ImageSharp.PixelFormats;

namespace WaxMapArt.Comparison;

public interface IColorComparison
{
    public double GetColorDifference(Rgb24 color1, Rgb24 color2);
}