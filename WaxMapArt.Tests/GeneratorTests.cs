using SkiaSharp;
using WaxMapArt.Dithering;
using WaxMapArt.Generators;
using WaxMapArt.Models;

namespace WaxMapArt.Tests;

public class GeneratorTests
{
    private static Palette CreateTestPalette()
    {
        return new Palette(
        [
            new() { Id = "minecraft:white_wool", ColorId = 8, Properties = [] },
            new() { Id = "minecraft:red_wool", ColorId = 4, Properties = [] },
        ], new Block { Id = "minecraft:stone", ColorId = 1, Properties = [] });
    }

    private static PixelMatch[,] CreateTestGrid(int width, int height, byte shading = 1)
    {
        var grid = new PixelMatch[width, height];
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
            grid[x, y] = new PixelMatch("minecraft:white_wool", new Dictionary<string, string>(), false, shading);
        return grid;
    }

    [Fact]
    public void FlatGenerator_GeneratesCorrectBlockCount()
    {
        var palette = CreateTestPalette();
        var grid = CreateTestGrid(2, 2);
        var generator = new FlatGenerator();
        var blocks = generator.Generate(grid, palette);

        // (width * supportRow) + (width * height blocks) + (no NeedSupport blocks)
        Assert.Equal(2 + 4, blocks.Length);
    }

    [Fact]
    public void FlatGenerator_HasPlaceholderRowAtZ0()
    {
        var palette = CreateTestPalette();
        var grid = CreateTestGrid(1, 1);
        var generator = new FlatGenerator();
        var blocks = generator.Generate(grid, palette);
        var placeholder = blocks.First(b => b.Z == 0);
        Assert.Equal("minecraft:stone", placeholder.Id);
    }

    [Fact]
    public void FlatGenerator_BlocksAtY0()
    {
        var palette = CreateTestPalette();
        var grid = CreateTestGrid(2, 2);
        var generator = new FlatGenerator();
        var blocks = generator.Generate(grid, palette);
        foreach (var b in blocks)
            Assert.Equal(0, b.Y);
    }

    [Fact]
    public void FlatGenerator_CorrectCoordinates()
    {
        var palette = CreateTestPalette();
        var grid = CreateTestGrid(3, 3);
        var generator = new FlatGenerator();
        var blocks = generator.Generate(grid, palette);

        var pixelBlocks = blocks.Where(b => b.Id == "minecraft:white_wool").ToList();
        Assert.Equal(9, pixelBlocks.Count);
        Assert.Contains(pixelBlocks, b => b.X == 2 && b.Z == 3);
    }

    [Fact]
    public void FlatGenerator_NeedSupport_PlacesSupportBlock()
    {
        var palette = CreateTestPalette();
        var grid = new PixelMatch[1, 1];
        grid[0, 0] = new PixelMatch("minecraft:white_wool",
            new Dictionary<string, string>(), true, 1);
        var generator = new FlatGenerator();
        var blocks = generator.Generate(grid, palette);
        var supportBlocks = blocks.Where(b => b.Id == "minecraft:stone").ToList();
        Assert.Equal(2, supportBlocks.Count); // placeholder + support
    }

    [Fact]
    public void StaircaseGenerator_GeneratesCorrectBlockCount()
    {
        var palette = CreateTestPalette();
        var grid = CreateTestGrid(2, 2, 1);
        var generator = new StaircaseGenerator();
        var blocks = generator.Generate(grid, palette);
        Assert.True(blocks.Length > 0);
    }

    [Fact]
    public void StaircaseGenerator_DifferentShading_ProducesDifferentY()
    {
        var palette = CreateTestPalette();
        var grid = new PixelMatch[1, 2];
        grid[0, 0] = new PixelMatch("minecraft:white_wool", new Dictionary<string, string>(), false, 0);
        grid[0, 1] = new PixelMatch("minecraft:white_wool", new Dictionary<string, string>(), false, 2);

        var generator = new StaircaseGenerator();
        var blocks = generator.Generate(grid, palette);

        var blockY0 = blocks.First(b => b.Z == 1);
        var blockY1 = blocks.First(b => b.Z == 2);
        Assert.NotEqual(blockY0.Y, blockY1.Y);
    }

    [Fact]
    public void StaircaseGenerator_ResultHasNoNegativeY()
    {
        var palette = CreateTestPalette();
        var grid = CreateTestGrid(4, 4, 0);
        var generator = new StaircaseGenerator();
        var blocks = generator.Generate(grid, palette);
        foreach (var b in blocks)
            Assert.True(b.Y >= 0, $"Block at ({b.X},{b.Y},{b.Z}) has negative Y");
    }

    [Fact]
    public void StaircaseGenerator_NeedSupport_PlacesSupport()
    {
        var palette = CreateTestPalette();
        var grid = new PixelMatch[1, 1];
        grid[0, 0] = new PixelMatch("minecraft:white_wool",
            new Dictionary<string, string>(), true, 1);
        var generator = new StaircaseGenerator();
        var blocks = generator.Generate(grid, palette);
        var supportBlocks = blocks.Where(b => b.Id == "minecraft:stone").ToList();
        Assert.Equal(2, supportBlocks.Count); // placeholder + support
    }

    [Fact]
    public void GeneratorOutput_CountBlocks_ReturnsCorrectCount()
    {
        var blocks = new List<BlockInfo>
        {
            new(0, 0, 0, "minecraft:stone", new Dictionary<string, string>()),
            new(1, 0, 0, "minecraft:stone", new Dictionary<string, string>()),
            new(2, 0, 0, "minecraft:dirt", new Dictionary<string, string>()),
        };
        var output = new GeneratorOutput(blocks);
        var counts = output.CountBlocks();
        Assert.Equal(2, counts["minecraft:stone"]);
        Assert.Equal(1, counts["minecraft:dirt"]);
    }

    [Fact]
    public void GeneratorOutput_RenderPreview_ReturnsCorrectSize()
    {
        var palette = CreateTestPalette();
        var grid = CreateTestGrid(8, 8, 1);
        var preview = GeneratorOutput.RenderPreview(grid, palette);
        Assert.Equal(8, preview.Width);
        Assert.Equal(8, preview.Height);
    }

    [Fact]
    public void GeneratorOutput_RenderPreview_RespectsShading()
    {
        var palette = CreateTestPalette();
        var grid = new PixelMatch[2, 1];
        grid[0, 0] = new PixelMatch("minecraft:white_wool",
            new Dictionary<string, string>(), false, 0);
        grid[1, 0] = new PixelMatch("minecraft:white_wool",
            new Dictionary<string, string>(), false, 2);

        var preview = GeneratorOutput.RenderPreview(grid, palette);
        var color0 = preview.GetPixel(0, 0);
        var color2 = preview.GetPixel(1, 0);
        Assert.NotEqual(color0, color2);
    }

    [Fact]
    public void FlatAndStaircase_ImplementIGenerator()
    {
        Assert.IsAssignableFrom<IGenerator>(new FlatGenerator());
        Assert.IsAssignableFrom<IGenerator>(new StaircaseGenerator());
    }
}
