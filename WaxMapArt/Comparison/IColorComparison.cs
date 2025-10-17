using SkiaSharp;

namespace WaxMapArt.Comparison;

public interface IColorComparison
{
    public double GetColorDifference(SKColor color1, SKColor color2);
}

public enum ComparisonMode
{
    Rgb,
    Cie76,
    Cie94,
    CieDe2000,
    Cmc,
    DeltaEITP
}