using SkiaSharp;
using WaxMapArt.Models;

namespace WaxMapArt.Dithering;

internal readonly struct PaletteEntry
{
    public SKColor Color { get; }
    public PixelMatch Match { get; }

    public PaletteEntry(SKColor color, PixelMatch match)
    {
        Color = color;
        Match = match;
    }
}

internal static class PaletteResolver
{
    private const double M0 = 0.71;
    private const double M1 = 0.86;

    public static PaletteEntry[] Resolve(Palette palette, StaircaseMode mode)
    {
        var isStaircase = mode is StaircaseMode.Staircase or StaircaseMode.AdaptiveStaircase;

        if (!isStaircase)
            return BuildFlatEntries(palette);

        return BuildStaircaseEntries(palette);
    }

    private static PaletteEntry[] BuildFlatEntries(Palette palette)
    {
        var entries = new PaletteEntry[palette.Blocks.Count];
        for (var i = 0; i < palette.Blocks.Count; i++)
        {
            var block = palette.Blocks[i];
            var color = MapColors.Multiply(MapColors.GetColor(block.ColorId), M1);
            entries[i] = new PaletteEntry(color,
                new PixelMatch(block.Id, block.Properties, block.NeedSupport, 1));
        }

        return entries;
    }

    private static PaletteEntry[] BuildStaircaseEntries(Palette palette)
    {
        var entries = new PaletteEntry[palette.Blocks.Count * 3];
        var idx = 0;
        foreach (var block in palette.Blocks)
        {
            var baseColor = MapColors.GetColor(block.ColorId);

            entries[idx++] = new PaletteEntry(MapColors.Multiply(baseColor, M0),
                new PixelMatch(block.Id, block.Properties, block.NeedSupport, 0));

            entries[idx++] = new PaletteEntry(MapColors.Multiply(baseColor, M1),
                new PixelMatch(block.Id, block.Properties, block.NeedSupport, 1));

            entries[idx++] = new PaletteEntry(baseColor,
                new PixelMatch(block.Id, block.Properties, block.NeedSupport, 2));
        }

        return entries;
    }
}
