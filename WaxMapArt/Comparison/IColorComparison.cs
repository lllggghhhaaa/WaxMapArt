using SkiaSharp;

namespace WaxMapArt.Comparison;

public interface IColorComparison
{
    public double GetColorDifference(SKColor color1, SKColor color2);
}