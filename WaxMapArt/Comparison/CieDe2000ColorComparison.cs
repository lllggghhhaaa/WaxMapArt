using SkiaSharp;
using WaxMapArt.Entities;

namespace WaxMapArt.Comparison;

public class CieDe2000ColorComparison : IColorComparison
{
    public double GetColorDifference(SKColor color1, SKColor color2)
    {
        var lab1 = Lab.FromSKColor(color1);
        var lab2 = Lab.FromSKColor(color2);

        var k257 = Math.Pow(25, 7);

        var c1Ab = Math.Sqrt(Math.Pow(lab1.A, 2) + Math.Pow(lab1.B, 2));
        var c2Ab = Math.Sqrt(Math.Pow(lab2.A, 2) + Math.Pow(lab2.B, 2));
        var cAbAverage = (c1Ab + c2Ab) / 2;

        var g = 0.5 * (1 - Math.Sqrt(Math.Pow(cAbAverage, 7) / (Math.Pow(cAbAverage, 7) + k257)));
        var a1 = (1 + g) * lab1.A;
        var a2 = (1 + g) * lab2.A;

        var c1 = Math.Sqrt(Math.Pow(a1, 2) + Math.Pow(lab1.B, 2));
        var c2 = Math.Sqrt(Math.Pow(a2, 2) + Math.Pow(lab2.B, 2));

        double h1;

        if (lab1.B == 0 && a1 == 0)
            h1 = 0;
        else if (a1 >= 0)
            h1 = Math.Atan2(lab1.B, a1) + 2 * Math.PI;
        else
            h1 = Math.Atan2(lab1.B, a1);

        double h2;

        if (lab2.B == 0 && a2 == 0)
            h2 = 0;
        else if (a2 >= 0)
            h2 = Math.Atan2(lab2.B, a2) + 2 * Math.PI;
        else
            h2 = Math.Atan2(lab2.B, a2);

        var dL = lab2.L - lab1.L;
        var dC = c2 - c1;
        var dh = h2 - h1;

        if (c1 * c2 == 0)
            dh = 0;
        else switch (dh)
        {
            case > Math.PI:
                dh -= 2 * Math.PI;
                break;
            case < -Math.PI:
                dh += 2 * Math.PI;
                break;
        }

        var dH2 = 2 * Math.Sqrt(c1 * c2) * Math.Sin(dh / 2);
        var lAverage = (lab1.L + lab2.L) / 2;
        var cAverage = (c1 + c2) / 2;
        var dh3 = Math.Abs(h1 - h2);
        var sh = h1 + h2;
        var c1C2 = c1 * c2;

        var hAverage = dh3 switch
        {
            <= Math.PI when c1C2 != 0 => sh / 2,
            > Math.PI when sh < 2 * Math.PI && c1C2 != 0 => sh / 2 + Math.PI,
            > Math.PI when sh >= 2 * Math.PI && c1C2 != 0 => sh / 2 - Math.PI,
            _ => sh
        };

        var T = 1 - 0.17 * Math.Cos(hAverage - Math.PI / 6) + 0.24 * Math.Cos(2 * hAverage) +
            0.32 * Math.Cos(3 * hAverage + Math.PI / 30) - 0.2 * Math.Cos(4 * hAverage - 63 * Math.PI / 180);
        var hAverageDeg = hAverage * 180 / Math.PI;

        switch (hAverageDeg)
        {
            case < 0:
                hAverageDeg += 360;
                break;
            case > 360:
                hAverageDeg -= 360;
                break;
        }

        var dTheta = 30 * Math.Exp(-Math.Pow((hAverageDeg - 275) / 25, 2));
        var rC = 2 * Math.Sqrt(Math.Pow(cAverage, 7) / (Math.Pow(cAverage, 7) + k257));
        var l50 = Math.Pow(lAverage - 50, 2);
        var sL = 1 + 0.015 * l50 / Math.Sqrt(20 + l50);
        var sC = 1 + 0.045 * cAverage;
        var sH = 1 + 0.015 * cAverage * T;
        var rT = -Math.Sin(dTheta * Math.PI / 90) * rC;

        var fL = dL / sL;
        var fC = dC / sC;
        var fH = dH2 / sH;
        
        return Math.Sqrt(Math.Pow(fL, 2) + Math.Pow(fC, 2) + Math.Pow(fH, 2) + rT * fC * fH);
    }
}