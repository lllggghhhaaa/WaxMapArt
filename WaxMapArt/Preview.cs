using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace WaxMapArt;

public class Preview
{
    public ComparisonMethod Method = ComparisonMethod.Cie76;
    public Size MapSize = new(1, 1);
    public Size OutputSize = new(128, 128);
    public Palette ColorPalette;

    public Preview(Palette colorPalette) => ColorPalette = colorPalette;

    public PreviewOutput GeneratePreview(Image<Rgb24> input)
    {
        Size size = MapSize * 128;
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
        
        return new PreviewOutput(outImage, usedBlocks);
    }
}

public record struct PreviewOutput(Image<Rgb24> Image, Dictionary<int, int> BlockList);

public record struct Size(int X, int Y)
{
    public static Size operator *(Size size, int multiplier) =>
        new(size.X * multiplier,
            size.Y * multiplier);
}