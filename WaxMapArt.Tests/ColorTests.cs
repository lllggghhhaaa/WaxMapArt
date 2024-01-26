namespace WaxMapArt.Tests;

public class ColorTests
{
    [Fact]
    public void TestRgbToLab()
    {
        var color = new WaxColor(255, 71, 76);
        double[] expected = [ 58.30967734192984, 68.81282563889386, 39.25754525189781 ];
        var result = color.ToLab();
        
        Assert.Equal(result.L, expected[0]);
        Assert.Equal(result.A, expected[1]);
        Assert.Equal(result.B, expected[2]);
    }

    [Fact]
    public void TestCie76Distance()
    {
        var color1 = new WaxColor(36, 34, 62);
        var color2 = new WaxColor(235, 26, 65);

        double expected = 91.015448439009106;
        double result = color1.Cie76Distance(color2);
        
        Assert.Equal(expected, result);
    }
    
    [Fact]
    public void TestCieDe2000Distance()
    {
        var color1 = new WaxColor(36, 34, 62);
        var color2 = new WaxColor(235, 26, 65);
        
        double expected = 43.385965641648035;
        double result = color1.CieDe2000Distance(color2);
        
        Assert.Equal(expected, result);
    }
}