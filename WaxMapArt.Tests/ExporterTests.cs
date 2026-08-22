using WaxMapArt.Exporters;
using WaxMapArt.Models;

namespace WaxMapArt.Tests;

public class ExporterTests
{
    private static Palette CreateTestPalette()
    {
        return new Palette(
        [
            new() { Id = "minecraft:white_wool", ColorId = 8, Properties = [] },
            new() { Id = "minecraft:red_wool", ColorId = 4, Properties = [] },
        ], new Block { Id = "minecraft:stone", ColorId = 1, Properties = [] });
    }

    private static List<BlockInfo> CreateTestBlocks()
    {
        return
        [
            new(0, 0, 0, "minecraft:stone", new Dictionary<string, string>()),
            new(0, 0, 1, "minecraft:white_wool", new Dictionary<string, string>()),
            new(1, 0, 0, "minecraft:stone", new Dictionary<string, string>()),
            new(1, 0, 1, "minecraft:red_wool", new Dictionary<string, string>()),
        ];
    }

    [Fact]
    public void VanillaExporter_ReturnsNonEmptyStream()
    {
        var palette = CreateTestPalette();
        var blocks = CreateTestBlocks();
        var exporter = new VanillaExporter();
        using var stream = exporter.Export(blocks, palette);
        Assert.NotNull(stream);
        Assert.True(stream.Length > 0);
    }

    [Fact]
    public void VanillaExporter_StreamIsReadable()
    {
        var palette = CreateTestPalette();
        var blocks = CreateTestBlocks();
        var exporter = new VanillaExporter();
        using var stream = exporter.Export(blocks, palette);
        Assert.True(stream.CanRead);
    }

    [Fact]
    public void VanillaExporter_FileExtension_IsNbt()
    {
        var exporter = new VanillaExporter();
        Assert.Equal(".nbt", exporter.FileExtension);
    }

    [Fact]
    public void LitematicaExporter_ReturnsNonEmptyStream()
    {
        var palette = CreateTestPalette();
        var blocks = CreateTestBlocks();
        var exporter = new LitematicaExporter();
        using var stream = exporter.Export(blocks, palette);
        Assert.NotNull(stream);
        Assert.True(stream.Length > 0);
    }

    [Fact]
    public void LitematicaExporter_StreamIsReadable()
    {
        var palette = CreateTestPalette();
        var blocks = CreateTestBlocks();
        var exporter = new LitematicaExporter();
        using var stream = exporter.Export(blocks, palette);
        Assert.True(stream.CanRead);
    }

    [Fact]
    public void LitematicaExporter_FileExtension_IsLitematic()
    {
        var exporter = new LitematicaExporter();
        Assert.Equal(".litematic", exporter.FileExtension);
    }

    [Fact]
    public void VanillaExporter_EmptyBlocks_ReturnsStream()
    {
        var palette = CreateTestPalette();
        var exporter = new VanillaExporter();
        using var stream = exporter.Export([], palette);
        Assert.True(stream.Length > 0);
    }

    [Fact]
    public void LitematicaExporter_EmptyBlocks_ReturnsStream()
    {
        var palette = CreateTestPalette();
        var exporter = new LitematicaExporter();
        using var stream = exporter.Export([], palette);
        Assert.True(stream.Length > 0);
    }

    [Fact]
    public void VanillaExporter_WithProperties_DoesNotThrow()
    {
        var palette = CreateTestPalette();
        var blocks = new List<BlockInfo>
        {
            new(0, 0, 0, "minecraft:oak_log",
                new Dictionary<string, string> { ["axis"] = "y" })
        };
        var exporter = new VanillaExporter();
        using var stream = exporter.Export(blocks, palette);
        Assert.True(stream.Length > 0);
    }

    [Fact]
    public void LitematicaExporter_WithProperties_DoesNotThrow()
    {
        var palette = CreateTestPalette();
        var blocks = new List<BlockInfo>
        {
            new(0, 0, 0, "minecraft:oak_log",
                new Dictionary<string, string> { ["axis"] = "y" })
        };
        var exporter = new LitematicaExporter();
        using var stream = exporter.Export(blocks, palette);
        Assert.True(stream.Length > 0);
    }

    [Fact]
    public void BothExporters_ImplementIExporter()
    {
        Assert.IsAssignableFrom<IExporter>(new VanillaExporter());
        Assert.IsAssignableFrom<IExporter>(new LitematicaExporter());
    }

    [Fact]
    public void VanillaExporter_LargeBlocks_DoesNotThrow()
    {
        var palette = CreateTestPalette();
        var blocks = new List<BlockInfo>();
        for (var i = 0; i < 100; i++)
            blocks.Add(new BlockInfo(i % 10, i / 100, i / 10,
                i % 2 == 0 ? "minecraft:white_wool" : "minecraft:red_wool",
                new Dictionary<string, string>()));

        var exporter = new VanillaExporter();
        using var stream = exporter.Export(blocks, palette);
        Assert.True(stream.Length > 0);
    }

    [Fact]
    public void LitematicaExporter_LargeBlocks_DoesNotThrow()
    {
        var palette = CreateTestPalette();
        var blocks = new List<BlockInfo>();
        for (var i = 0; i < 100; i++)
            blocks.Add(new BlockInfo(i % 10, i / 100, i / 10,
                i % 2 == 0 ? "minecraft:white_wool" : "minecraft:red_wool",
                new Dictionary<string, string>()));

        var exporter = new LitematicaExporter();
        using var stream = exporter.Export(blocks, palette);
        Assert.True(stream.Length > 0);
    }
}
