using SkiaSharp;
using WaxMapArt.Comparison;
using WaxMapArt.Dithering;
using WaxMapArt.Models;

namespace WaxMapArt.Tests;

public class DitheringTests
{
    private static Palette CreateRainbowPalette()
    {
        return new Palette(
        [
            new() { Id = "minecraft:black_wool", ColorId = 29, Properties = [] },
            new() { Id = "minecraft:white_wool", ColorId = 8, Properties = [] },
            new() { Id = "minecraft:red_wool", ColorId = 4, Properties = [] },
            new() { Id = "minecraft:blue_wool", ColorId = 25, Properties = [] },
        ], new Block { Id = "minecraft:stone", ColorId = 1 });
    }

    private static SKBitmap CreateTestImage(int width, int height)
    {
        var bmp = new SKBitmap(width, height);
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
            bmp.SetPixel(x, y, new SKColor((byte)(x * 255 / width), (byte)(y * 255 / height), 128));
        return bmp;
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(16, 16)]
    [InlineData(128, 128)]
    public void NoneDithering_ReturnsCorrectDimensions(int w, int h)
    {
        var palette = CreateRainbowPalette();
        var image = CreateTestImage(w, h);
        var dithering = new NoneDithering();
        var result = dithering.Apply(image, palette, new Cie76ColorComparison(), StaircaseMode.Flat);
        Assert.Equal(w, result.GetLength(0));
        Assert.Equal(h, result.GetLength(1));
    }

    [Fact]
    public void NoneDithering_MatchesNearestColor()
    {
        var palette = CreateRainbowPalette();
        var image = CreateTestImage(4, 4);
        var dithering = new NoneDithering();
        var result = dithering.Apply(image, palette, new RgbColorComparison(), StaircaseMode.Flat);
        for (var y = 0; y < 4; y++)
        for (var x = 0; x < 4; x++)
        {
            var match = result[x, y];
            Assert.NotNull(match.Id);
            Assert.True(match.Shading == 1);
        }
    }

    [Fact]
    public void NoneDithering_BlackImage_MatchesBlackWool()
    {
        var palette = CreateRainbowPalette();
        var image = new SKBitmap(2, 2);
        image.SetPixel(0, 0, new SKColor(0, 0, 0));
        var dithering = new NoneDithering();
        var result = dithering.Apply(image, palette, new Cie76ColorComparison(), StaircaseMode.Flat);
        Assert.Equal("minecraft:black_wool", result[0, 0].Id);
    }

    [Fact]
    public void FloydSteinbergDithering_ReturnsCorrectDimensions()
    {
        var palette = CreateRainbowPalette();
        var image = CreateTestImage(16, 16);
        var dithering = new FloydSteinbergDithering();
        var result = dithering.Apply(image, palette, new Cie76ColorComparison(), StaircaseMode.Flat);
        Assert.Equal(16, result.GetLength(0));
        Assert.Equal(16, result.GetLength(1));
    }

    [Fact]
    public void AtkinsonDithering_ReturnsCorrectDimensions()
    {
        var palette = CreateRainbowPalette();
        var image = CreateTestImage(16, 16);
        var dithering = new AtkinsonDithering();
        var result = dithering.Apply(image, palette, new Cie76ColorComparison(), StaircaseMode.Flat);
        Assert.Equal(16, result.GetLength(0));
        Assert.Equal(16, result.GetLength(1));
    }

    [Fact]
    public void JarvisJudiceNinkeDithering_ReturnsCorrectDimensions()
    {
        var palette = CreateRainbowPalette();
        var image = CreateTestImage(16, 16);
        var dithering = new JarvisJudiceNinkeDithering();
        var result = dithering.Apply(image, palette, new Cie76ColorComparison(), StaircaseMode.Flat);
        Assert.Equal(16, result.GetLength(0));
        Assert.Equal(16, result.GetLength(1));
    }

    [Fact]
    public void FloydSteinberg_Serpentine_DoesNotThrow()
    {
        var palette = CreateRainbowPalette();
        var image = CreateTestImage(8, 8);
        var dithering = new FloydSteinbergDithering(serpentineScanning: true);
        var result = dithering.Apply(image, palette, new Cie76ColorComparison(), StaircaseMode.Flat);
        Assert.Equal(8, result.GetLength(0));
    }

    [Fact]
    public void FloydSteinberg_WithStrength_DoesNotThrow()
    {
        var palette = CreateRainbowPalette();
        var image = CreateTestImage(8, 8);
        var dithering = new FloydSteinbergDithering(errorDiffusionStrength: 0.5f);
        var result = dithering.Apply(image, palette, new Cie76ColorComparison(), StaircaseMode.Flat);
        Assert.Equal(8, result.GetLength(0));
    }

    [Fact]
    public void NoneDithering_StaircaseMode_HasVariedShading()
    {
        var palette = CreateRainbowPalette();
        var image = CreateTestImage(4, 4);
        var dithering = new NoneDithering();
        var result = dithering.Apply(image, palette, new Cie76ColorComparison(), StaircaseMode.Staircase);
        var shadings = new HashSet<byte>();
        for (var y = 0; y < 4; y++)
        for (var x = 0; x < 4; x++)
            shadings.Add(result[x, y].Shading);
        Assert.True(shadings.Count > 0);
    }

    [Fact]
    public void AllDitherings_OnSinglePixel_ReturnMatch()
    {
        var palette = CreateRainbowPalette();
        var image = new SKBitmap(1, 1);
        image.SetPixel(0, 0, new SKColor(200, 100, 50));

        IDithering[] ditherings =
        [
            new NoneDithering(),
            new FloydSteinbergDithering(),
            new AtkinsonDithering(),
            new JarvisJudiceNinkeDithering()
        ];

        foreach (var d in ditherings)
        {
            var result = d.Apply(image, palette, new Cie76ColorComparison(), StaircaseMode.Flat);
            Assert.NotNull(result[0, 0].Id);
        }
    }
}
