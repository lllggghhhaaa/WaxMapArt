using SkiaSharp;

namespace WaxMapArt.Entities;

public record struct Lab(double L, double A, double B)
{
    public static Lab FromSKColor(SKColor color)
    {
        double r = color.Red / 255d;
        double g = color.Green / 255d;
        double b = color.Blue / 255d;

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
}