using SkiaSharp;

namespace WaxMapArt.Tests;

public class MapColorsTests
{
    [Fact]
    public void GetColor_ValidId_ReturnsColor()
    {
        var color = MapColors.GetColor(1);
        Assert.Equal(127, color.Red);
        Assert.Equal(178, color.Green);
        Assert.Equal(56, color.Blue);
    }

    [Fact]
    public void GetColor_Zero_ReturnsBlack()
    {
        var color = MapColors.GetColor(0);
        Assert.Equal(0, color.Red);
        Assert.Equal(0, color.Green);
        Assert.Equal(0, color.Blue);
    }

    [Fact]
    public void GetColor_OutOfRange_ReturnsDefault()
    {
        var color = MapColors.GetColor(999);
        Assert.Equal(0, color.Red);
        Assert.Equal(0, color.Green);
        Assert.Equal(0, color.Blue);
    }

    [Fact]
    public void GetColor_Negative_ReturnsDefault()
    {
        var color = MapColors.GetColor(-1);
        Assert.Equal(0, color.Red);
        Assert.Equal(0, color.Green);
        Assert.Equal(0, color.Blue);
    }

    [Fact]
    public void Multiply_ScalesColor()
    {
        var color = new SKColor(100, 150, 200);
        var result = MapColors.Multiply(color, 0.5);
        Assert.Equal(50, result.Red);
        Assert.Equal(75, result.Green);
        Assert.Equal(100, result.Blue);
    }

    [Fact]
    public void Multiply_Zero_ReturnsBlack()
    {
        var color = new SKColor(100, 150, 200);
        var result = MapColors.Multiply(color, 0);
        Assert.Equal(0, result.Red);
        Assert.Equal(0, result.Green);
        Assert.Equal(0, result.Blue);
    }

    [Fact]
    public void Multiply_FactorOne_ReturnsSame()
    {
        var color = new SKColor(100, 150, 200);
        var result = MapColors.Multiply(color, 1.0);
        Assert.Equal(color, result);
    }

    [Fact]
    public void AllNonBlackColors_HaveNonZeroSum()
    {
        for (var i = 1; i <= 61; i++)
        {
            var color = MapColors.GetColor(i);
            Assert.NotEqual(0, color.Red + color.Green + color.Blue);
        }
    }

    [Fact]
    public void ColorCount_Is62()
    {
        Assert.Equal(new SKColor(0, 0, 0), MapColors.GetColor(0));
    }

    [Fact]
    public void WhiteColor_IsCorrect()
    {
        var color = MapColors.GetColor(8);
        Assert.Equal(255, color.Red);
        Assert.Equal(255, color.Green);
        Assert.Equal(255, color.Blue);
    }
}
