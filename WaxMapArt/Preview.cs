using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using WaxMapArt.ImageProcessing;
using WaxMapArt.ImageProcessing.Dithering;

namespace WaxMapArt;

public class Preview
{
    public ComparisonMethod Method = ComparisonMethod.Cie76;
    public WaxSize MapSize = new(1, 1);
    public WaxSize OutputSize = new(128, 128);
    public DitheringType Dithering = DitheringType.None;
    public Palette ColorPalette;

    public Preview(Palette colorPalette) => ColorPalette = colorPalette;

    public PreviewOutput GeneratePreviewStaircase(Image<Rgb24> input)
    {
        WaxSize size = MapSize * 128;
        var usedBlocks = new List<int>();
        var outImage = new Image<Rgb24>(size.X, size.Y);
        
        new ImageProcessor(size, Dithering).Process(ref input);

        var colors = new List<BlockColor>();
        
        foreach (var (_, info) in ColorPalette.Colors)
        {
            Rgb24 baseColor = MapColors.BaseColors[info.MapId];
            
            colors.Add(new BlockColor(baseColor.Multiply(MapColors.M0), info));
            colors.Add(new BlockColor(baseColor.Multiply(MapColors.M1), info));
            colors.Add(new BlockColor(baseColor, info));
        }

        Parallel.For(0, size.X, x =>
        {
            for (int y = 0; y < size.Y; y++)
            {
                Rgb24 inputColor = input[x, y];
                Rgb24 nearest = inputColor.Nearest(colors.Select(blockColor => blockColor.Color), Method);

                outImage[x, y] = nearest;
                int id = colors.Find(blockColor => blockColor.Color == nearest).Info.MapId;

                usedBlocks.Add(id);
            }
        });
        
        outImage.Mutate(ctx => ctx.Resize(OutputSize.X, OutputSize.Y));

        var blockCount = new Dictionary<int, int>();

        blockCount.Add(ColorPalette.PlaceholderBlock.MapId, size.X);
        foreach (var block in usedBlocks)
        {
            if (blockCount.ContainsKey(block)) blockCount[block]++;
            else blockCount.Add(block, 1);
        }
        

        return new PreviewOutput(outImage, blockCount.OrderByDescending(bc => bc.Value).ToDictionary());
    }

    public PreviewOutput GeneratePreviewFlat(Image<Rgb24> input)
    {
        WaxSize size = MapSize * 128;
        var usedBlocks = new List<int>();
        var outImage = new Image<Rgb24>(size.X, size.Y);
        
        new ImageProcessor(size, Dithering).Process(ref input);

        var colors = new List<BlockColor>();
        foreach (var (_, info) in ColorPalette.Colors)
            colors.Add(new BlockColor(MapColors.BaseColors[info.MapId].Multiply(MapColors.M1), info));

        Parallel.For(0, size.X, x =>
        {
            for (int y = 0; y < size.Y; y++)
            {
                Rgb24 inputColor = input[x, y];
                Rgb24 nearest = inputColor.Nearest(colors.Select(blockColor => blockColor.Color), Method);

                outImage[x, y] = nearest;
                int id = colors.Find(blockColor => blockColor.Color == nearest).Info.MapId;

                usedBlocks.Add(id);
            }
        });

        outImage.Mutate(ctx => ctx.Resize(OutputSize.X, OutputSize.Y));

        var blockCount = new Dictionary<int, int>();

        foreach (var block in usedBlocks)
        {
            if (blockCount.ContainsKey(block)) blockCount[block]++;
            else blockCount.Add(block, 1);
        }

        return new PreviewOutput(outImage, blockCount.OrderByDescending(bc => bc.Value).ToDictionary());
    }
}

public record struct PreviewOutput(Image<Rgb24> Image, Dictionary<int, int> BlockList);

public record struct WaxSize(int X, int Y)
{
    public static WaxSize operator *(WaxSize waxSize, int multiplier) =>
        new(waxSize.X * multiplier,
            waxSize.Y * multiplier);
}