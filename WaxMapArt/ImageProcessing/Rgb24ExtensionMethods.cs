using SixLabors.ImageSharp.PixelFormats;

namespace WaxMapArt.ImageProcessing;

public static class Rgb24ExtensionMethods
{
    public static Rgb24 Sum(this Rgb24 a, Rgb24 b) =>
        new(
            (byte)(a.R + b.R),
            (byte)(a.G + b.G),
            (byte)(a.B + b.B));
    
    public static Rgb24 Subtract(this Rgb24 a, Rgb24 b) =>
        new(
            (byte)(a.R - b.R),
            (byte)(a.G - b.G),
            (byte)(a.B - b.B));
 
    public static Rgb24 Multiply(this Rgb24 a, Rgb24 b) =>
        new(
            (byte)(a.R * b.R),
            (byte)(a.G * b.G),
            (byte)(a.B * b.B));
    
    public static Rgb24 Multiply(this Rgb24 color, double multiplier) =>
        new(
            (byte)(color.R / multiplier),
            (byte)(color.G / multiplier),
            (byte)(color.B / multiplier));
    
    public static Rgb24 Divide(this Rgb24 a, Rgb24 b) =>
        new(
            (byte)(a.R / b.R),
            (byte)(a.G / b.G),
            (byte)(a.B / b.B));
    
    public static Rgb24 Divide(this Rgb24 color, double divider) =>
        new(
            (byte)(color.R / divider),
            (byte)(color.G / divider),
            (byte)(color.B / divider));
}