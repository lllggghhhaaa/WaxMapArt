using SixLabors.ImageSharp.PixelFormats;
using WaxMapArt.Entities;

namespace WaxMapArt.Comparison;

public class Cie76ColorComparison : IColorComparison
{
    public double GetColorDifference(Rgb24 color1, Rgb24 color2)
    {
        var lab1 = Lab.FromRgb24(color1);
        var lab2 = Lab.FromRgb24(color2);
        
        return Math.Sqrt(Math.Pow(lab1.L - lab2.L, 2) +
                         Math.Pow(lab1.A - lab2.A, 2) +
                         Math.Pow(lab1.B - lab2.B, 2));
    }
}