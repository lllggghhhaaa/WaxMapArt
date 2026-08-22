using SkiaSharp;

namespace WaxMapArt;

public static class MapColors
{
    private static readonly SKColor[] Colors =
    [
        new(0, 0, 0),
        new(127, 178, 56),
        new(247, 233, 163),
        new(199, 199, 199),
        new(255, 0, 0),
        new(160, 160, 255),
        new(167, 167, 167),
        new(0, 124, 0),
        new(255, 255, 255),
        new(164, 168, 184),
        new(151, 109, 77),
        new(112, 112, 112),
        new(64, 64, 255),
        new(143, 119, 72),
        new(255, 252, 245),
        new(216, 127, 51),
        new(178, 76, 216),
        new(102, 153, 216),
        new(229, 229, 51),
        new(127, 204, 25),
        new(242, 127, 165),
        new(76, 76, 76),
        new(153, 153, 153),
        new(76, 127, 153),
        new(127, 63, 178),
        new(51, 76, 178),
        new(102, 76, 51),
        new(102, 127, 51),
        new(153, 51, 51),
        new(25, 25, 25),
        new(250, 238, 77),
        new(92, 219, 213),
        new(74, 128, 255),
        new(0, 217, 58),
        new(129, 86, 49),
        new(112, 2, 0),
        new(209, 177, 161),
        new(159, 82, 36),
        new(149, 87, 108),
        new(112, 108, 138),
        new(186, 133, 36),
        new(103, 117, 53),
        new(160, 77, 78),
        new(57, 41, 35),
        new(135, 107, 98),
        new(87, 92, 92),
        new(122, 73, 88),
        new(76, 62, 92),
        new(76, 50, 35),
        new(76, 82, 42),
        new(142, 60, 46),
        new(37, 22, 16),
        new(189, 48, 49),
        new(148, 63, 97),
        new(92, 25, 29),
        new(22, 126, 134),
        new(58, 142, 140),
        new(86, 44, 62),
        new(20, 180, 133),
        new(100, 100, 100),
        new(216, 175, 147),
        new(127, 167, 150)
    ];

    public static SKColor GetColor(int colorId) =>
        (uint)colorId < (uint)Colors.Length ? Colors[colorId] : Colors[0];

    public static SKColor Multiply(SKColor color, double factor) =>
        new((byte)(color.Red * factor), (byte)(color.Green * factor), (byte)(color.Blue * factor));
}
