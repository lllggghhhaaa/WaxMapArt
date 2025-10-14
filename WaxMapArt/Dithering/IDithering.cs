using SkiaSharp;
using WaxMapArt.Comparison;

namespace WaxMapArt.Dithering;

public interface IDithering
{ 
    public SKBitmap ApplyDithering(SKBitmap image, Palette palette, IColorComparison colorComparison, bool staircase = false);
}

public enum DitheringMode
{
    None,
    Atkinson,
    FloydSteinberg,
    JarvisJudiceNinke
}