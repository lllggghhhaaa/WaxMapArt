using SkiaSharp;

namespace WaxMapArt.Comparison;

public class DeltaEITPColorComparison : IColorComparison
{
    public double GetColorDifference(SKColor c1, SKColor c2)
    {
        var ictcp1 = SrgbToICtCp(c1);
        var ictcp2 = SrgbToICtCp(c2);

        var di = ictcp1.I - ictcp2.I;
        var dct = ictcp1.Ct - ictcp2.Ct;
        var dcp = ictcp1.Cp - ictcp2.Cp;

        return Math.Sqrt(di * di + dct * dct + dcp * dcp);
    }

    private record struct ICtCpTriplet(double I, double Ct, double Cp);

    private static ICtCpTriplet SrgbToICtCp(SKColor color)
    {
        var r = SrgbToLinear(color.Red / 255.0);
        var g = SrgbToLinear(color.Green / 255.0);
        var b = SrgbToLinear(color.Blue / 255.0);

        LinearRgbToXyz(r, g, b, out var X, out var Y, out var Z);

        var L = 0.3593 * X + 0.6976 * Y - 0.0359 * Z;
        var M = -0.1921 * X + 1.1005 * Y + 0.0754 * Z;
        var S = 0.0071 * X + 0.0748 * Y + 0.8433 * Z;

        var Lp = InversePQ(L);
        var Mp = InversePQ(M);
        var Sp = InversePQ(S);

        var iVal = (2048.0 * Lp + 2048.0 * Mp + 0.0 * Sp) / 4096.0;
        var ct = (6610.0 * Lp - 13613.0 * Mp + 7003.0 * Sp) / 4096.0;
        var cp = (17933.0 * Lp - 17390.0 * Mp - 543.0 * Sp) / 4096.0;

        return new ICtCpTriplet(iVal * 720.0, ct * 360.0, cp * 720.0);
    }

    private static double InversePQ(double Y)
    {
        const double m1 = 0.1593017578125;
        const double m2 = 78.84375;
        const double c1 = 0.8359375;
        const double c2 = 18.8515625;
        const double c3 = 18.6875;

        var Ycl = Math.Clamp(Y, 0.0, 1.0);
        var Ym1 = Math.Pow(Ycl, m1);
        var numerator = c1 + c2 * Ym1;
        var denominator = 1.0 + c3 * Ym1;
        var val = Math.Pow(numerator / denominator, m2);
        return double.IsFinite(val) ? val : 0.0;
    }
    
    private static double SrgbToLinear(double c)
    {
        if (c <= 0.04045) return c / 12.92;
        return Math.Pow((c + 0.055) / 1.055, 2.4);
    }
    
    private static void LinearRgbToXyz(double r, double g, double b, out double x, out double y, out double z)
    {
        x = 0.4124564 * r + 0.3575761 * g + 0.1804375 * b;
        y = 0.2126729 * r + 0.7151522 * g + 0.0721750 * b;
        z = 0.0193339 * r + 0.1191920 * g + 0.9503041 * b;
    }
}