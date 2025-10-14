using SkiaSharp;

namespace WaxMapArt.Comparison;

public class RgbColorComparison : IColorComparison
{
    public double GetColorDifference(SKColor color1, SKColor color2)
        => Math.Sqrt(Math.Pow((double)color1.Red - color2.Red, 2) +
                     Math.Pow((double)color1.Green - color2.Green, 2) +
                     Math.Pow((double)color1.Blue - color2.Blue, 2));
}