using SkiaSharp;
using WaxMapArt.Comparison;
using WaxMapArt.Dithering;
using WaxMapArt.Entities;

namespace WaxMapArt.Utils;

public static class ColorUtils
{
    internal const double M0 = .71; 
    internal const double M1 = .86;

    internal static SKColor FindNearestColor(SKColor originalColor,
        SKColor[] palette,
        SKColor[] flatPalette,
        IColorComparison colorComparison,
        StaircaseMode staircaseMode,
        double threshold) =>
        staircaseMode is StaircaseMode.AdaptiveStaircase
            ? FindNearestColorAdaptive(originalColor, palette, flatPalette, colorComparison, threshold)
            : FindNearestColor(originalColor, palette, colorComparison);

    internal static SKColor FindNearestColor(SKColor originalColor, SKColor[] palette, IColorComparison colorComparison)
    {
        var nearestColor = palette.First();
        var shortestDistance = double.MaxValue;

        foreach (var color in palette)
        {
            var distance = colorComparison.GetColorDifference(originalColor, color);
            if (!(distance < shortestDistance)) continue;
            shortestDistance = distance;
            nearestColor = color;
        }

        return nearestColor;
    }

    internal static SKColor FindNearestColorAdaptive(
        SKColor originalColor,
        SKColor[] palette,
        SKColor[] flatPalette,
        IColorComparison colorComparison,
        double threshold)
    {
        var bestColor = palette[0];
        var bestDistance = colorComparison.GetColorDifference(originalColor, bestColor);

        for (var i = 1; i < palette.Length; i++)
        {
            var distance = colorComparison.GetColorDifference(originalColor, palette[i]);
            if (!(distance < bestDistance)) continue;
            bestDistance = distance;
            bestColor = palette[i];
        }

        var bestFlatColor = flatPalette[0];
        var bestFlatDistance = colorComparison.GetColorDifference(originalColor, bestFlatColor);

        for (var i = 1; i < flatPalette.Length; i++)
        {
            var distance = colorComparison.GetColorDifference(originalColor, flatPalette[i]);
            if (!(distance < bestFlatDistance)) continue;
            bestFlatDistance = distance;
            bestFlatColor = flatPalette[i];
        }

        return Math.Abs(bestFlatDistance - bestDistance) <= threshold ? bestFlatColor : bestColor;
    }


    public static SKColor[] GetPaletteColors(Palette palette, bool staircase = false)
    {
        return staircase
            ? palette.Colors
                .SelectMany(color =>
                {
                    var baseColor = MapIdToInfo(color.MapId).Color;
                    return new[]
                    {
                        baseColor.Multiply(M0),
                        baseColor.Multiply(M1),
                        baseColor
                    };
                }).ToArray()
            : palette.Colors
                .Select(color => MapIdToInfo(color.MapId).Color.Multiply(M1))
                .ToArray();
    }
    
    public static List<BlockData> GetPaletteBlocks(Palette palette, bool staircase = false)
    {
        return staircase
            ? palette.Colors
                .SelectMany(color =>
                {
                    var baseColor = new BlockData(color.Id, MapIdToInfo(color.MapId).Color, 2,
                        color.GeneratorProperties, color.Properties);
                    var paletteColor1 = baseColor.Clone();
                    var paletteColor2 = baseColor.Clone();

                    paletteColor1.Color = baseColor.Color.Multiply(M1);
                    paletteColor1.Shading = 1;
                    paletteColor2.Color = baseColor.Color.Multiply(M0);
                    paletteColor2.Shading = 0;

                    return new[] { paletteColor2, paletteColor1, baseColor };
                }).ToList()
            : palette.Colors
                .Select(color => new BlockData(color.Id, MapIdToInfo(color.MapId).Color, 1, color.GeneratorProperties,
                    color.Properties)).ToList();
    }

    public static string ToHexColor(this SKColor color)
        => $"#{color.Red:X2}{color.Green:X2}{color.Blue:X2}";
    
    public static SKColor Multiply(this SKColor color, double factor)
        => new((byte)(color.Red * factor), (byte)(color.Green * factor), (byte)(color.Blue * factor));

    public static MapIdInfo MapIdToInfo(int id)
    {
        return id switch
        {
            0 => new MapIdInfo(0, "NONE", new SKColor(0, 0, 0)),
            1 => new MapIdInfo(1, "GRASS", new SKColor(127, 178, 56)),
            2 => new MapIdInfo(2, "SAND", new SKColor(247, 233, 163)),
            3 => new MapIdInfo(3, "WOOL", new SKColor(199, 199, 199)),
            4 => new MapIdInfo(4, "FIRE", new SKColor(255, 0, 0)),
            5 => new MapIdInfo(5, "ICE", new SKColor(160, 160, 255)),
            6 => new MapIdInfo(6, "METAL", new SKColor(167, 167, 167)),
            7 => new MapIdInfo(7, "PLANT", new SKColor(0, 124, 0)),
            8 => new MapIdInfo(8, "SNOW", new SKColor(255, 255, 255)),
            9 => new MapIdInfo(9, "CLAY", new SKColor(164, 168, 184)),
            10 => new MapIdInfo(10, "DIRT", new SKColor(151, 109, 77)),
            11 => new MapIdInfo(11, "STONE", new SKColor(112, 112, 112)),
            12 => new MapIdInfo(12, "WATER", new SKColor(64, 64, 255)),
            13 => new MapIdInfo(13, "WOOD", new SKColor(143, 119, 72)),
            14 => new MapIdInfo(14, "QUARTZ", new SKColor(255, 252, 245)),
            15 => new MapIdInfo(15, "COLOR_ORANGE", new SKColor(216, 127, 51)),
            16 => new MapIdInfo(16, "COLOR_MAGENTA", new SKColor(178, 76, 216)),
            17 => new MapIdInfo(17, "COLOR_LIGHT_BLUE", new SKColor(102, 153, 216)),
            18 => new MapIdInfo(18, "COLOR_YELLOW", new SKColor(229, 229, 51)),
            19 => new MapIdInfo(19, "COLOR_LIGHT_GREEN", new SKColor(127, 204, 25)),
            20 => new MapIdInfo(20, "COLOR_PINK", new SKColor(242, 127, 165)),
            21 => new MapIdInfo(21, "COLOR_GRAY", new SKColor(76, 76, 76)),
            22 => new MapIdInfo(22, "COLOR_LIGHT_GRAY", new SKColor(153, 153, 153)),
            23 => new MapIdInfo(23, "COLOR_CYAN", new SKColor(76, 127, 153)),
            24 => new MapIdInfo(24, "COLOR_PURPLE", new SKColor(127, 63, 178)),
            25 => new MapIdInfo(25, "COLOR_BLUE", new SKColor(51, 76, 178)),
            26 => new MapIdInfo(26, "COLOR_BROWN", new SKColor(102, 76, 51)),
            27 => new MapIdInfo(27, "COLOR_GREEN", new SKColor(102, 127, 51)),
            28 => new MapIdInfo(28, "COLOR_RED", new SKColor(153, 51, 51)),
            29 => new MapIdInfo(29, "COLOR_BLACK", new SKColor(25, 25, 25)),
            30 => new MapIdInfo(30, "GOLD", new SKColor(250, 238, 77)),
            31 => new MapIdInfo(31, "DIAMOND", new SKColor(92, 219, 213)),
            32 => new MapIdInfo(32, "LAPIS", new SKColor(74, 128, 255)),
            33 => new MapIdInfo(33, "EMERALD", new SKColor(0, 217, 58)),
            34 => new MapIdInfo(34, "PODZOL", new SKColor(129, 86, 49)),
            35 => new MapIdInfo(35, "NETHER", new SKColor(112, 2, 0)),
            36 => new MapIdInfo(36, "TERRACOTTA_WHITE", new SKColor(209, 177, 161)),
            37 => new MapIdInfo(37, "TERRACOTTA_ORANGE", new SKColor(159, 82, 36)),
            38 => new MapIdInfo(38, "TERRACOTTA_MAGENTA", new SKColor(149, 87, 108)),
            39 => new MapIdInfo(39, "TERRACOTTA_LIGHT_BLUE", new SKColor(112, 108, 138)),
            40 => new MapIdInfo(40, "TERRACOTTA_YELLOW", new SKColor(186, 133, 36)),
            41 => new MapIdInfo(41, "TERRACOTTA_LIGHT_GREEN", new SKColor(103, 117, 53)),
            42 => new MapIdInfo(42, "TERRACOTTA_PINK", new SKColor(160, 77, 78)),
            43 => new MapIdInfo(43, "TERRACOTTA_GRAY", new SKColor(57, 41, 35)),
            44 => new MapIdInfo(44, "TERRACOTTA_LIGHT_GRAY", new SKColor(135, 107, 98)),
            45 => new MapIdInfo(45, "TERRACOTTA_CYAN", new SKColor(87, 92, 92)),
            46 => new MapIdInfo(46, "TERRACOTTA_PURPLE", new SKColor(122, 73, 88)),
            47 => new MapIdInfo(47, "TERRACOTTA_BLUE", new SKColor(76, 62, 92)),
            48 => new MapIdInfo(48, "TERRACOTTA_BROWN", new SKColor(76, 50, 35)),
            49 => new MapIdInfo(49, "TERRACOTTA_GREEN", new SKColor(76, 82, 42)),
            50 => new MapIdInfo(50, "TERRACOTTA_RED", new SKColor(142, 60, 46)),
            51 => new MapIdInfo(51, "TERRACOTTA_BLACK", new SKColor(37, 22, 16)),
            52 => new MapIdInfo(52, "CRIMSON_NYLIUM", new SKColor(189, 48, 49)),
            53 => new MapIdInfo(53, "CRIMSON_STEM", new SKColor(148, 63, 97)),
            54 => new MapIdInfo(54, "CRIMSON_HYPHAE", new SKColor(92, 25, 29)),
            55 => new MapIdInfo(55, "WARPED_NYLIUM", new SKColor(22, 126, 134)),
            56 => new MapIdInfo(56, "WARPED_STEM", new SKColor(58, 142, 140)),
            57 => new MapIdInfo(57, "WARPED_HYPHAE", new SKColor(86, 44, 62)),
            58 => new MapIdInfo(58, "WARPED_WART_BLOCK", new SKColor(20, 180, 133)),
            59 => new MapIdInfo(59, "DEEPSLATE", new SKColor(100, 100, 100)),
            60 => new MapIdInfo(60, "RAW_IRON", new SKColor(216, 175, 147)),
            61 => new MapIdInfo(61, "GLOW_LICHEN", new SKColor(127, 167, 150)),
            _ => new MapIdInfo(-1, "Unknown", new SKColor(255, 255, 255))
        };
    }
}

public struct BlockData(string id, SKColor color, byte shading, GeneratorProperties generatorProperties, Dictionary<string, string> properties)
{
    public string Id = id;
    public SKColor Color = color;
    public byte Shading = shading;
    public Dictionary<string, string> Properties = properties;
    public GeneratorProperties GeneratorProperties = generatorProperties;

    public BlockData Clone() => new(id: Id, color: Color, shading: Shading, properties: Properties,
        generatorProperties: GeneratorProperties);
}