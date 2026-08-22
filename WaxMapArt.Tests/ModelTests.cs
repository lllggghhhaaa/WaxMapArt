using WaxMapArt.Models;

namespace WaxMapArt.Tests;

public class ModelTests
{
    [Fact]
    public void BlockInfo_PropertiesStored()
    {
        var props = new Dictionary<string, string> { ["facing"] = "north" };
        var info = new BlockInfo(1, 2, 3, "minecraft:stone", props);
        Assert.Equal(1, info.X);
        Assert.Equal(2, info.Y);
        Assert.Equal(3, info.Z);
        Assert.Equal("minecraft:stone", info.Id);
        Assert.Equal("north", info.Properties["facing"]);
    }

    [Fact]
    public void BlockInfo_Implicit_PropertiesEmpty()
    {
        var props = new Dictionary<string, string>();
        var info = new BlockInfo(0, 0, 0, "minecraft:air", props);
        Assert.Empty(info.Properties);
    }

    [Fact]
    public void PixelMatch_PropertiesStored()
    {
        var props = new Dictionary<string, string> { ["axis"] = "y" };
        var match = new PixelMatch("minecraft:oak_log", props, true, 2);
        Assert.Equal("minecraft:oak_log", match.Id);
        Assert.True(match.NeedSupport);
        Assert.Equal(2, match.Shading);
        Assert.Equal("y", match.Properties["axis"]);
    }

    [Fact]
    public void PixelMatch_DefaultShading_IsZero()
    {
        var match = new PixelMatch("minecraft:air",
            new Dictionary<string, string>(), false, 0);
        Assert.Equal(0, match.Shading);
        Assert.False(match.NeedSupport);
    }

    [Fact]
    public void Block_Defaults()
    {
        var block = new Block();
        Assert.NotNull(block.Properties);
        Assert.Empty(block.Properties);
        Assert.False(block.NeedSupport);
    }

    [Fact]
    public void Palette_StoresBlocks()
    {
        var blocks = new List<Block>
        {
            new() { Id = "minecraft:stone", ColorId = 1 },
            new() { Id = "minecraft:dirt", ColorId = 2 }
        };
        var support = new Block { Id = "minecraft:glass", ColorId = 0 };
        var palette = new Palette(blocks, support);
        Assert.Equal(2, palette.Blocks.Count);
        Assert.Equal("minecraft:glass", palette.SupportBlock.Id);
    }

    [Fact]
    public void Lab_ValuesStored()
    {
        var lab = new Lab(50.0, 20.0, -30.0);
        Assert.Equal(50.0, lab.L);
        Assert.Equal(20.0, lab.A);
        Assert.Equal(-30.0, lab.B);
    }
}
