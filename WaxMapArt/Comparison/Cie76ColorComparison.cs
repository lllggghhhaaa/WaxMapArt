using SkiaSharp;
using WaxMapArt.Entities;

namespace WaxMapArt.Comparison;

public class Cie76ColorComparison : IColorComparison
{
    public double GetColorDifference(SKColor color1, SKColor color2)
    {
        var lab1 = Lab.FromSKColor(color1);
        var lab2 = Lab.FromSKColor(color2);
        
        return Math.Sqrt(Math.Pow(lab1.L - lab2.L, 2) +
                         Math.Pow(lab1.A - lab2.A, 2) +
                         Math.Pow(lab1.B - lab2.B, 2));
    }
}