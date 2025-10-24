using SkiaSharp;
using WaxMapArt.Comparison;

namespace WaxMapArt.Dithering;

public interface IDithering
{ 
    public SKBitmap ApplyDithering(SKBitmap image, Palette palette, IColorComparison colorComparison, StaircaseMode staircaseMode, double threshold);
}

public enum DitheringMode
{
    None,
    Atkinson,
    FloydSteinberg,
    JarvisJudiceNinke
}

public enum StaircaseMode
{
    Flat,
    Staircase,
    AdaptiveStaircase
}