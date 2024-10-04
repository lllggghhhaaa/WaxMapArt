using SixLabors.ImageSharp.PixelFormats;
using WaxMapArt.Entities;

namespace WaxMapArt.Comparison;

public class CieDe2000ColorComparison : IColorComparison
{
    public double GetColorDifference(Rgb24 color1, Rgb24 color2)
    {
        Lab lab1 = Lab.FromRgb24(color1);
        Lab lab2 = Lab.FromRgb24(color2);

        double hAverage;
        double k257 = Math.Pow(25, 7);

        double c1Ab = Math.Sqrt(Math.Pow(lab1.A, 2) + Math.Pow(lab1.B, 2));
        double c2Ab = Math.Sqrt(Math.Pow(lab2.A, 2) + Math.Pow(lab2.B, 2));
        double cAbAverage = (c1Ab + c2Ab) / 2;

        double g = 0.5 * (1 - Math.Sqrt(Math.Pow(cAbAverage, 7) / (Math.Pow(cAbAverage, 7) + k257)));
        double a1 = (1 + g) * lab1.A;
        double a2 = (1 + g) * lab2.A;

        double c1 = Math.Sqrt(Math.Pow(a1, 2) + Math.Pow(lab1.B, 2));
        double c2 = Math.Sqrt(Math.Pow(a2, 2) + Math.Pow(lab2.B, 2));

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

        double dL = lab2.L - lab1.L;
        double dC = c2 - c1;
        double dh = h2 - h1;

        if (c1 * c2 == 0)
            dh = 0;
        else if (dh > Math.PI)
            dh -= 2 * Math.PI;
        else if (dh < -Math.PI) dh += 2 * Math.PI;

        double dH2 = 2 * Math.Sqrt(c1 * c2) * Math.Sin(dh / 2);
        double lAverage = (lab1.L + lab2.L) / 2;
        double cAverage = (c1 + c2) / 2;
        double dh3 = Math.Abs(h1 - h2);
        double sh = h1 + h2;
        double c1C2 = c1 * c2;

        if (dh3 <= Math.PI && c1C2 != 0)
            hAverage = sh / 2;
        else if (dh3 > Math.PI && sh < 2 * Math.PI && c1C2 != 0)
            hAverage = sh / 2 + Math.PI;
        else if (dh3 > Math.PI && sh >= 2 * Math.PI && c1C2 != 0)
            hAverage = sh / 2 - Math.PI;
        else
            hAverage = sh;

        double T = 1 - 0.17 * Math.Cos(hAverage - Math.PI / 6) + 0.24 * Math.Cos(2 * hAverage) +
            0.32 * Math.Cos(3 * hAverage + Math.PI / 30) - 0.2 * Math.Cos(4 * hAverage - 63 * Math.PI / 180);
        double hAverageDeg = hAverage * 180 / Math.PI;

        if (hAverageDeg < 0)
            hAverageDeg += 360;
        else if (hAverageDeg > 360) hAverageDeg -= 360;

        double dTheta = 30 * Math.Exp(-Math.Pow((hAverageDeg - 275) / 25, 2));
        double rC = 2 * Math.Sqrt(Math.Pow(cAverage, 7) / (Math.Pow(cAverage, 7) + k257));
        double l50 = Math.Pow(lAverage - 50, 2);
        double sL = 1 + 0.015 * l50 / Math.Sqrt(20 + l50);
        double sC = 1 + 0.045 * cAverage;
        double sH = 1 + 0.015 * cAverage * T;
        double rT = -Math.Sin(dTheta * Math.PI / 90) * rC;

        double fL = dL / sL;
        double fC = dC / sC;
        double fH = dH2 / sH;
        
        return Math.Sqrt(Math.Pow(fL, 2) + Math.Pow(fC, 2) + Math.Pow(fH, 2) + rT * fC * fH);
    }
}