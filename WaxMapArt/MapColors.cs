using SixLabors.ImageSharp.PixelFormats;

namespace WaxMapArt;

public static class MapColors
{
    public static readonly Rgb24[] BaseColors =
    {
        new(255, 255, 255), // Transparent
        new(127, 178, 56),  // Grass
        new(247, 233, 163), // Sand
        new(199, 199, 199), // Wool
        new(255, 0, 0),     // Fire
        new(160, 160, 255), // Ice
        new(167, 167, 167), // Metal
        new(0, 124, 0),     // Plant
        new(255, 255, 255), // Snow
        new(164, 168, 184), // Clay
        new(151, 109, 77),  // Dirt
        new(112, 112, 112), // Stone
        new(64, 64, 255),   // Water
        new(143, 119, 72),  // Wood
        new(255, 252, 245), // Quartz
        new(216, 127, 51),  // Color Orange
        new(178, 76, 216),  // Color Magenta
        new(102, 153, 216), // Color Light Blue
        new(229, 229, 51),  // Color Yellow
        new(127, 204, 25),  // Color Light Green
        new(242, 127, 165), // Color Pink
        new(76, 76, 76),    // Color Gray
        new(153, 153, 153), // Color Light Gray
        new(76, 127, 153),  // Color Cyan
        new(127, 63, 178),  // Color Purple
        new(51, 76, 178),   // Color Blue
        new(102, 76, 51),   // Color Brown
        new(102, 127, 51),  // Color Green
        new(153, 51, 51),   // Color Red
        new(25, 25, 25),    // Color Black
        new(250, 238, 77),  // Gold
        new(92, 219, 213),  // Diamond
        new(74, 128, 255),  // Lapis
        new(0, 217, 58),    // Emerald
        new(129, 86, 49),   // Podzol
        new(112, 2, 0),     // Nether
        new(209, 177, 161), // Terracotta White
        new(159, 82, 36),   // Terracotta Orange
        new(149, 87, 108),  // Terracotta Magenta
        new(112, 108, 138), // Terracotta Light Blue
        new(186, 133, 36),  // Terracotta Yellow
        new(103, 117, 53),  // Terracotta Light Green
        new(160, 77, 78),   // Terracotta Pink
        new(57, 41, 35),    // Terracotta Gray
        new(135, 107, 98),  // Terracotta Light Gray
        new(87, 92, 92),    // Terracotta Cyan
        new(122, 73, 88),   // Terracotta Purple
        new(76, 62, 92),    // Terracotta Blue
        new(76, 50, 35),    // Terracotta Brown
        new(76, 82, 42),    // Terracotta Green
        new(142, 60, 46),   // Terracotta Red
        new(37, 22, 16),    // Terracotta Black
        new(189, 48, 49),   // Crimson Nylium
        new(148, 63, 97),   // Crimson Stem
        new(92, 25, 29),    // Crimson Hyphae
        new(22, 126, 134),  // Warped Nylium
        new(58, 142, 140),  // Warped Stem
        new(86, 44, 62),    // Warped Hyphae
        new(20, 180, 133),  // Warped Wart Block
        new(100, 100, 100), // Deepslate
        new(216, 175, 147), // Raw Iron
        new(127, 167, 150)  // Glow Lichen
    };

    public const double M0 = .71;
    public const double M1 = .86;
}

public enum MapNames
{
    Transparent,
    Grass,
    Sand,
    Wool,
    Fire,
    Ice,
    Metal,
    Plant,
    Snow,
    Clay,
    Dirt ,
    Stone,
    Water,
    Wood,
    Quartz,
    ColorOrange,
    ColorMagenta,
    ColorLightBlue,
    ColorYellow,
    ColorLightGreen,
    ColorPink,
    ColorGray ,
    ColorLightGray,
    ColorCyan,
    ColorPurple,
    ColorBlue,
    ColorBrown,
    ColorGreen,
    ColorRed,
    ColorBlack,
    Gold,
    Diamond,
    Lapis,
    Emerald,
    Podzol,
    Nether,
    TerracottaWhite,
    TerracottaOrange,
    TerracottaMagenta,
    TerracottaLightBlue,
    TerracottaYellow,
    TerracottaLightGreen,
    TerracottaPink,
    TerracottaGray,
    TerracottaLightGray,
    TerracottaCyan,
    TerracottaPurple,
    TerracottaBlue,
    TerracottaBrown,
    TerracottaGreen,
    TerracottaRed,
    TerracottaBlack,
    CrimsonNylium,
    CrimsonStem,
    CrimsonHyphae,
    WarpedNylium,
    WarpedStem,
    WarpedHyphae,
    WarpedWartBlock,
    Deepslate,
    RawIron,
    GlowLichen
}