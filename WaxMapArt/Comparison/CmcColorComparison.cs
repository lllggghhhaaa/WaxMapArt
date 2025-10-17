using SkiaSharp;
using WaxMapArt.Entities;

namespace WaxMapArt.Comparison;

public class CmcColorComparison(double lightnessRatio = 2, double chromaRatio = 1) : IColorComparison
{
    public double GetColorDifference(SKColor color1, SKColor color2)
    {
        var lab1 = Lab.FromSKColor(color1);
        var lab2 = Lab.FromSKColor(color2);

        var dL = lab1.L - lab2.L;
        var C1 = Math.Sqrt(lab1.A * lab1.A + lab1.B * lab1.B);
        var C2 = Math.Sqrt(lab2.A * lab2.A + lab2.B * lab2.B);
        var dC = C1 - C2;

        var da = lab1.A - lab2.A;
        var db = lab1.B - lab2.B;
        var dH_sq = da * da + db * db - dC * dC;
        if (dH_sq < 0) dH_sq = 0;
        var dH = Math.Sqrt(dH_sq);

        var F = Math.Sqrt(Math.Pow(C1, 4) / (Math.Pow(C1, 4) + 1900.0));
        var H1 = Math.Atan2(lab1.B, lab1.A) * 180.0 / Math.PI;
        if (H1 < 0) H1 += 360.0;

        double T;
        if (H1 >= 164 && H1 <= 345)
            T = 0.56 + Math.Abs(0.2 * Math.Cos((H1 + 168.0) * Math.PI / 180.0));
        else
            T = 0.36 + Math.Abs(0.4 * Math.Cos((H1 + 35.0) * Math.PI / 180.0));

        var SL = lab1.L < 16.0 ? 0.511 : 0.040975 * lab1.L / (1.0 + 0.01765 * lab1.L);
        var SC = 0.0638 * C1 / (1.0 + 0.0131 * C1) + 0.638;
        var SH = SC * (F * T + 1.0 - F);

        var vL = dL / (lightnessRatio * SL);
        var vC = dC / (chromaRatio * SC);
        var vH = dH / SH;

        return Math.Sqrt(vL * vL + vC * vC + vH * vH);
    }
    
}