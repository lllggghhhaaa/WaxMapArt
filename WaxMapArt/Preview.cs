using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace WaxMapArt;

public class Preview
{
    public ComparisonMethod Method = ComparisonMethod.Cie76;
    public WaxSize MapSize = new(1, 1);
    public WaxSize OutputSize = new(128, 128);
    public Palette ColorPalette;

    public Preview(Palette colorPalette) => ColorPalette = colorPalette;

    public PreviewOutput GeneratePreview(Image<Rgb24> input)
    {
        WaxSize size = MapSize * 128;
        var usedBlocks = new Dictionary<int, int>();
        var outImage = new Image<Rgb24>(size.X, size.Y);
        input.Mutate(ctx => ctx.Resize(size.X, size.Y));

        var colors = new List<BlockColor>();
        
        foreach (var (_, info) in ColorPalette.Colors)
        {
            Rgb24 baseColor = MapColors.BaseColors[info.MapId];
            
            colors.Add(new BlockColor(baseColor.Multiply(MapColors.M0), info));
            colors.Add(new BlockColor(baseColor.Multiply(MapColors.M1), info));
            colors.Add(new BlockColor(baseColor, info));
        }

        for (int x = 0; x < size.X; x++)
        for (int y = 0; y < size.Y; y++)
        {
            Rgb24 inputColor = input[x, y];
            Rgb24 nearest = inputColor.Nearest(colors.Select(blockColor => blockColor.Color), Method);

            outImage[x, y] = nearest;
            int id = colors.Find(blockColor => blockColor.Color == nearest).Info.MapId;

            if (usedBlocks.ContainsKey(id))
                usedBlocks[id]++;
            else
                usedBlocks.Add(id, 1);
        }
        
        outImage.Mutate(ctx => ctx.Resize(OutputSize.X, OutputSize.Y));
        usedBlocks = new Dictionary<int, int>(usedBlocks.OrderByDescending(pair => pair.Value));
        usedBlocks.Add(ColorPalette.PlaceholderBlock.MapId, size.X);
        
        return new PreviewOutput(outImage, usedBlocks);
    }
}

public record struct PreviewOutput(Image<Rgb24> Image, Dictionary<int, int> BlockList);

public record struct WaxSize(int X, int Y)
{
    public static WaxSize operator *(WaxSize waxSize, int multiplier) =>
        new(waxSize.X * multiplier,
            waxSize.Y * multiplier);
}