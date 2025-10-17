using SkiaSharp;
using WaxMapArt.Entities;

namespace WaxMapArt.Comparison;

public class Cie94ColorComparison(bool useTextTiles = false) : IColorComparison
{
    public double GetColorDifference(SKColor color1, SKColor color2)
    {
        var lab1 = Lab.FromSKColor(color1);
        var lab2 = Lab.FromSKColor(color2);

        var kL = useTextTiles ? 2.0 : 1.0;
        var K1 = useTextTiles ? 0.048 : 0.045;
        var K2 = useTextTiles ? 0.014 : 0.015;
        const double kC = 1.0;
        const double kH = 1.0;

        var dL = lab1.L - lab2.L;
        var C1 = Math.Sqrt(lab1.A * lab1.A + lab1.B * lab1.B);
        var C2 = Math.Sqrt(lab2.A * lab2.A + lab2.B * lab2.B);
        var dC = C1 - C2;

        var da = lab1.A - lab2.A;
        var db = lab1.B - lab2.B;
        var dH_sq = da * da + db * db - dC * dC;
        if (dH_sq < 0) dH_sq = 0;
        var dH = Math.Sqrt(dH_sq);

        var SL = 1.0;
        var SC = 1.0 + K1 * C1;
        var SH = 1.0 + K2 * C1;

        var vL = dL / (kL * SL);
        var vC = dC / (kC * SC);
        var vH = dH / (kH * SH);

        return Math.Sqrt(vL * vL + vC * vC + vH * vH);
    }
}