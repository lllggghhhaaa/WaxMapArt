using SkiaSharp;
using WaxMapArt.Comparison;

namespace WaxMapArt.Tests;

public class ColorComparisonTests
{
    [Fact]
    public void Cie76_SameColor_ReturnsZero()
    {
        var color = new SKColor(100, 150, 200);
        var comp = new Cie76ColorComparison();
        var diff = comp.GetColorDifference(color, color);
        Assert.Equal(0, diff, 5);
    }

    [Fact]
    public void Rgb_SameColor_ReturnsZero()
    {
        var color = new SKColor(100, 150, 200);
        var comp = new RgbColorComparison();
        var diff = comp.GetColorDifference(color, color);
        Assert.Equal(0, diff, 5);
    }

    [Fact]
    public void Cie94_SameColor_ReturnsZero()
    {
        var color = new SKColor(100, 150, 200);
        var comp = new Cie94ColorComparison();
        var diff = comp.GetColorDifference(color, color);
        Assert.Equal(0, diff, 5);
    }

    [Fact]
    public void CieDe2000_SameColor_ReturnsZero()
    {
        var color = new SKColor(100, 150, 200);
        var comp = new CieDe2000ColorComparison();
        var diff = comp.GetColorDifference(color, color);
        Assert.Equal(0, diff, 5);
    }

    [Fact]
    public void Cmc_SameColor_ReturnsZero()
    {
        var color = new SKColor(100, 150, 200);
        var comp = new CmcColorComparison();
        var diff = comp.GetColorDifference(color, color);
        Assert.Equal(0, diff, 5);
    }

    [Fact]
    public void DeltaEITP_SameColor_ReturnsZero()
    {
        var color = new SKColor(100, 150, 200);
        var comp = new DeltaEITPColorComparison();
        var diff = comp.GetColorDifference(color, color);
        Assert.Equal(0, diff, 5);
    }

    [Fact]
    public void Cie76_BlackVsWhite_LargeDifference()
    {
        var black = new SKColor(0, 0, 0);
        var white = new SKColor(255, 255, 255);
        var comp = new Cie76ColorComparison();
        var diff = comp.GetColorDifference(black, white);
        Assert.True(diff > 50);
    }

    [Fact]
    public void Rgb_BlackVsWhite_ReturnsSqrtOfThree255s()
    {
        var black = new SKColor(0, 0, 0);
        var white = new SKColor(255, 255, 255);
        var comp = new RgbColorComparison();
        var diff = comp.GetColorDifference(black, white);
        var expected = Math.Sqrt(3 * 255.0 * 255.0);
        Assert.Equal(expected, diff, 1);
    }

    [Fact]
    public void Cie76_NearColors_SmallDifference()
    {
        var color1 = new SKColor(100, 100, 100);
        var color2 = new SKColor(102, 101, 99);
        var comp = new Cie76ColorComparison();
        var diff = comp.GetColorDifference(color1, color2);
        Assert.True(diff < 5);
    }

    [Fact]
    public void AllComparisons_ProducePositiveValues()
    {
        var color1 = new SKColor(50, 100, 200);
        var color2 = new SKColor(200, 100, 50);

        IColorComparison[] comparisons =
        [
            new Cie76ColorComparison(),
            new Cie94ColorComparison(),
            new CieDe2000ColorComparison(),
            new CmcColorComparison(),
            new DeltaEITPColorComparison(),
            new RgbColorComparison()
        ];

        foreach (var comp in comparisons)
        {
            var diff = comp.GetColorDifference(color1, color2);
            Assert.True(diff >= 0, $"{comp.GetType().Name} produced negative difference: {diff}");
        }
    }

    [Fact]
    public void Cie94_TextTilesMode_DifferentFromGraphicArts()
    {
        var color1 = new SKColor(50, 100, 200);
        var color2 = new SKColor(200, 100, 50);
        var compTextiles = new Cie94ColorComparison(useTextTiles: true);
        var compGraphicArts = new Cie94ColorComparison(useTextTiles: false);
        var diffTextiles = compTextiles.GetColorDifference(color1, color2);
        var diffGraphicArts = compGraphicArts.GetColorDifference(color1, color2);
        Assert.NotEqual(diffTextiles, diffGraphicArts);
    }
}
