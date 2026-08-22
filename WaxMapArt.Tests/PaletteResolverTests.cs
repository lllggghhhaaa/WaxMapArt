using WaxMapArt.Dithering;
using WaxMapArt.Models;

namespace WaxMapArt.Tests;

public class PaletteResolverTests
{
    private static Palette CreateTestPalette()
    {
        var blocks = new List<Block>
        {
            new() { Id = "minecraft:white_wool", ColorId = 3, Properties = new() },
            new() { Id = "minecraft:red_wool", ColorId = 28, Properties = new() }
        };
        return new Palette(blocks, new Block { Id = "minecraft:stone", ColorId = 1 });
    }

    [Fact]
    public void Resolve_Flat_ReturnsOneEntryPerBlock()
    {
        var palette = CreateTestPalette();
        var entries = PaletteResolver.Resolve(palette, StaircaseMode.Flat);
        Assert.Equal(2, entries.Length);
    }

    [Fact]
    public void Resolve_Flat_AllShadingIsOne()
    {
        var palette = CreateTestPalette();
        var entries = PaletteResolver.Resolve(palette, StaircaseMode.Flat);
        foreach (var entry in entries)
            Assert.Equal(1, entry.Match.Shading);
    }

    [Fact]
    public void Resolve_Staircase_ReturnsThreeEntriesPerBlock()
    {
        var palette = CreateTestPalette();
        var entries = PaletteResolver.Resolve(palette, StaircaseMode.Staircase);
        Assert.Equal(6, entries.Length);
    }

    [Fact]
    public void Resolve_AdaptiveStaircase_SameAsStaircase()
    {
        var palette = CreateTestPalette();
        var staircase = PaletteResolver.Resolve(palette, StaircaseMode.Staircase);
        var adaptive = PaletteResolver.Resolve(palette, StaircaseMode.AdaptiveStaircase);
        Assert.Equal(staircase.Length, adaptive.Length);
    }

    [Fact]
    public void Resolve_Staircase_HasAllThreeShadings()
    {
        var palette = CreateTestPalette();
        var entries = PaletteResolver.Resolve(palette, StaircaseMode.Staircase);
        var shadings = entries.Select(e => e.Match.Shading).Distinct().OrderBy(s => s).ToArray();
        Assert.Equal([0, 1, 2], shadings);
    }

    [Fact]
    public void Resolve_Staircase_ShadingColorsAreDifferent()
    {
        var palette = CreateTestPalette();
        var entries = PaletteResolver.Resolve(palette, StaircaseMode.Staircase);

        var block0Entries = entries.Where(e => e.Match.Id == "minecraft:white_wool").ToArray();
        Assert.Equal(3, block0Entries.Length);
        Assert.NotEqual(block0Entries[0].Color, block0Entries[1].Color);
        Assert.NotEqual(block0Entries[1].Color, block0Entries[2].Color);
    }

    [Fact]
    public void Resolve_Flat_ColorIsM1Shade()
    {
        var palette = CreateTestPalette();
        var entries = PaletteResolver.Resolve(palette, StaircaseMode.Flat);
        var baseColor = MapColors.GetColor(3);
        var expected = MapColors.Multiply(baseColor, 0.86);
        Assert.Equal(expected, entries[0].Color);
    }

    [Fact]
    public void Resolve_PreservesProperties()
    {
        var blocks = new List<Block>
        {
            new() { Id = "minecraft:oak_log", ColorId = 3,
                Properties = new() { ["axis"] = "y" }, NeedSupport = true }
        };
        var palette = new Palette(blocks, new Block { Id = "minecraft:stone", ColorId = 1 });
        var entries = PaletteResolver.Resolve(palette, StaircaseMode.Flat);
        Assert.Equal("y", entries[0].Match.Properties["axis"]);
        Assert.True(entries[0].Match.NeedSupport);
    }

    [Fact]
    public void Resolve_EmptyPalette_ReturnsEmpty()
    {
        var palette = new Palette(new List<Block>(), new Block { Id = "minecraft:stone", ColorId = 1 });
        var entries = PaletteResolver.Resolve(palette, StaircaseMode.Flat);
        Assert.Empty(entries);
    }
}
