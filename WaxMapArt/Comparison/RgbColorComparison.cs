using SixLabors.ImageSharp.PixelFormats;

namespace WaxMapArt.Comparison;

public class RgbColorComparison : IColorComparison
{
    public double GetColorDifference(Rgb24 color1, Rgb24 color2)
        => Math.Sqrt(Math.Pow((double)color1.R - color2.R, 2) +
                     Math.Pow((double)color1.G - color2.G, 2) +
                     Math.Pow((double)color1.B - color2.B, 2));
}