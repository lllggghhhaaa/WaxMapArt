using Newtonsoft.Json;
using SixLabors.ImageSharp.PixelFormats;

namespace WaxMapArt;

public struct WaxColor(byte r, byte g, byte b)
{
    [JsonProperty("r")] public byte R = r;
    [JsonProperty("g")] public byte G = g;
    [JsonProperty("b")] public byte B = b;

    public WaxColor() : this(0, 0, 0)
    {
    }

    public static WaxColor FromRgb24(Rgb24 color) => new WaxColor(color.R, color.G, color.B);


    public WaxColor Nearest(IEnumerable<WaxColor> colors, ComparisonMethod method)
    {
        WaxColor a = this;

        double ColorDistance(WaxColor color) =>
            method switch
            {
                ComparisonMethod.Rgb => a.RgbDistance(color),
                ComparisonMethod.CieDe2000 => a.CieDe2000Distance(color),
                _ => a.Cie76Distance(color)
            };

        return colors
            .Select(n => new { n, distance = ColorDistance(n) })
            .OrderBy(p => p.distance)
            .First().n;
    }

    public Lab ToLab()
    {
        double r = R / 255d;
        double g = G / 255d;
        double b = B / 255d;

        r = r > .04045 ? Math.Pow((r + .055) / 1.055, 2.4) : r / 12.92;
        g = g > .04045 ? Math.Pow((g + .055) / 1.055, 2.4) : g / 12.92;
        b = b > .04045 ? Math.Pow((b + .055) / 1.055, 2.4) : b / 12.92;

        double x = (r * .4124 + g * .3576 + b * .1805) / 0.95047;
        double y = (r * .2126 + g * .7152 + b * .0722) / 1.00000;
        double z = (r * .0193 + g * .1192 + b * .9505) / 1.08883;

        x = x > .008856 ? Math.Pow(x, 1d / 3) : 7.787 * x + 16d / 116;
        y = y > .008856 ? Math.Pow(y, 1d / 3) : 7.787 * y + 16d / 116;
        z = z > .008856 ? Math.Pow(z, 1d / 3) : 7.787 * z + 16d / 116;

        return new Lab(116 * y - 16, 500 * (x - y), 200 * (y - z));
    }

    public double CieDe2000Distance(WaxColor b)
    {
        Lab lab1 = ToLab();
        Lab lab2 = b.ToLab();

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
            hAverage = (h1 + h2) / 2;
        else if (dh3 > Math.PI && sh < 2 * Math.PI && c1C2 != 0)
            hAverage = (h1 + h2) / 2 + Math.PI;
        else if (dh3 > Math.PI && sh >= 2 * Math.PI && c1C2 != 0)
            hAverage = (h1 + h2) / 2 - Math.PI;
        else
            hAverage = h1 + h2;

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

    public double Cie76Distance(WaxColor b)
    {
        Lab labA = ToLab();
        Lab labB = b.ToLab();

        return Math.Sqrt(Math.Pow(labA.L - labB.L, 2) +
                         Math.Pow(labA.A - labB.A, 2) +
                         Math.Pow(labA.B - labB.B, 2));
    }

    public double RgbDistance(WaxColor b)
        => Math.Sqrt(Math.Pow((double)R - b.R, 2) +
                     Math.Pow((double)G - b.G, 2) +
                     Math.Pow((double)B - b.B, 2));


    public static WaxColor operator *(WaxColor color, double multiplier)
    => new ((byte)(color.R * multiplier),
            (byte)(color.G * multiplier),
            (byte)(color.B * multiplier));

    public bool IsEquals(WaxColor b) =>
        R == b.R &&
        G == b.G &&
        B == b.B;

    public Rgb24 ToRgb24() => new Rgb24(R, G, B);
}

public static class Rgb24ExtensionMethods
{
   
}

public struct Lab(double l, double a, double b)
{
    public double L = l;
    public double A = a;
    public double B = b;

    public override string ToString() => $"lab({L}, {A}, {B})";
}

public enum ComparisonMethod
{
    Rgb,
    Cie76,
    CieDe2000
}