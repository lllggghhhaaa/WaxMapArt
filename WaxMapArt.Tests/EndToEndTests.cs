using SkiaSharp;
using WaxMapArt.Comparison;
using WaxMapArt.Dithering;
using WaxMapArt.Exporters;
using WaxMapArt.Generators;
using WaxMapArt.Models;

namespace WaxMapArt.Tests;

public class EndToEndTests
{
    private static Palette CreateTestPalette()
    {
        return new Palette(
        [
            new() { Id = "minecraft:white_wool", ColorId = 8, Properties = [] },
            new() { Id = "minecraft:light_gray_wool", ColorId = 22, Properties = [] },
            new() { Id = "minecraft:gray_wool", ColorId = 21, Properties = [] },
            new() { Id = "minecraft:black_wool", ColorId = 29, Properties = [] },
            new() { Id = "minecraft:red_wool", ColorId = 4, Properties = [] },
            new() { Id = "minecraft:blue_wool", ColorId = 25, Properties = [] },
            new() { Id = "minecraft:green_wool", ColorId = 27, Properties = [] },
            new() { Id = "minecraft:yellow_wool", ColorId = 18, Properties = [] },
        ], new Block { Id = "minecraft:stone", ColorId = 1, Properties = [] });
    }

    private static SKBitmap CreateGradientImage(int size)
    {
        var bmp = new SKBitmap(size, size);
        for (var y = 0; y < size; y++)
        for (var x = 0; x < size; x++)
            bmp.SetPixel(x, y, new SKColor((byte)(x * 255 / size), (byte)(y * 255 / size), 128));
        return bmp;
    }

    [Fact]
    public void FullPipeline_FlatDitherExport_Succeeds()
    {
        var palette = CreateTestPalette();
        var image = CreateGradientImage(16);

        IDithering dithering = new FloydSteinbergDithering();
        var pixels = dithering.Apply(image, palette, new Cie76ColorComparison(), StaircaseMode.Flat);

        IGenerator generator = new FlatGenerator();
        var blocks = generator.Generate(pixels, palette);

        IExporter exporter = new VanillaExporter();
        using var stream = exporter.Export(blocks, palette);

        Assert.True(blocks.Length > 0);
        Assert.True(stream.Length > 0);
    }

    [Fact]
    public void FullPipeline_StaircaseDitherExport_Succeeds()
    {
        var palette = CreateTestPalette();
        var image = CreateGradientImage(16);

        IDithering dithering = new NoneDithering();
        var pixels = dithering.Apply(image, palette, new Cie76ColorComparison(), StaircaseMode.Staircase);

        IGenerator generator = new StaircaseGenerator();
        var blocks = generator.Generate(pixels, palette);

        IExporter exporter = new LitematicaExporter();
        using var stream = exporter.Export(blocks, palette);

        Assert.True(blocks.Length > 0);
        Assert.True(stream.Length > 0);
    }

    [Fact]
    public void FullPipeline_AllDitherings_ProduceBlocks()
    {
        var palette = CreateTestPalette();
        var image = CreateGradientImage(8);

        IDithering[] ditherings =
        [
            new NoneDithering(),
            new FloydSteinbergDithering(),
            new AtkinsonDithering(),
            new JarvisJudiceNinkeDithering()
        ];

        foreach (var dithering in ditherings)
        {
            var pixels = dithering.Apply(image, palette, new Cie76ColorComparison(), StaircaseMode.Flat);
            IGenerator generator = new FlatGenerator();
            var blocks = generator.Generate(pixels, palette);
            Assert.True(blocks.Length > 0, $"{dithering.GetType().Name} produced no blocks");
        }
    }

    [Fact]
    public void FullPipeline_WithPreview_Succeeds()
    {
        var palette = CreateTestPalette();
        var image = CreateGradientImage(8);

        var dithering = new NoneDithering();
        var pixels = dithering.Apply(image, palette, new Cie76ColorComparison(), StaircaseMode.Flat);

        var preview = GeneratorOutput.RenderPreview(pixels, palette);
        Assert.Equal(8, preview.Width);
        Assert.Equal(8, preview.Height);

        IGenerator generator = new FlatGenerator();
        var blocks = generator.Generate(pixels, palette);
        var counts = new GeneratorOutput(blocks).CountBlocks();
        Assert.True(counts.Values.Sum() > 0);
    }

    [Fact]
    public void FullPipeline_MultipleComparisons_AllWork()
    {
        var palette = CreateTestPalette();
        var image = CreateGradientImage(4);

        IColorComparison[] comparisons =
        [
            new Cie76ColorComparison(),
            new Cie94ColorComparison(),
            new RgbColorComparison(),
            new CmcColorComparison(),
        ];

        foreach (var comp in comparisons)
        {
            var pixels = new NoneDithering().Apply(image, palette, comp, StaircaseMode.Flat);
            var blocks = new FlatGenerator().Generate(pixels, palette);
            Assert.True(blocks.Length > 0, $"{comp.GetType().Name} produced no blocks");
        }
    }

    [Fact]
    public void FullPipeline_BothExporters_FromSameData()
    {
        var palette = CreateTestPalette();
        var image = CreateGradientImage(8);

        var pixels = new NoneDithering().Apply(image, palette, new Cie76ColorComparison(), StaircaseMode.Flat);
        var blocks = new FlatGenerator().Generate(pixels, palette);

        using var vanillaStream = new VanillaExporter().Export(blocks, palette);
        using var litematicaStream = new LitematicaExporter().Export(blocks, palette);

        Assert.True(vanillaStream.Length > 0);
        Assert.True(litematicaStream.Length > 0);
    }
}
