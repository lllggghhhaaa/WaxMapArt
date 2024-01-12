using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Collections.Concurrent;
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
        var size = MapSize * 128;
        var usedBlocks = new ConcurrentBag<BlockInfo>();
        var outImage = new Image<Rgb24>(size.X, size.Y);
        var pImage = new ImageProcessor(size, Dithering).Process(input);

        var colors = new List<BlockColor>();
        
        foreach (var (_, info) in ColorPalette.Colors)
        {
            var baseColor = info.Color;
            
            colors.Add(new BlockColor(baseColor * MapColors.M0, info));
            colors.Add(new BlockColor(baseColor * MapColors.M1, info));
            colors.Add(new BlockColor(baseColor, info));
        }

        Parallel.For(0, size.X, x =>
        {
            for (int y = 0; y < size.Y; y++)
            {
                var inputColor = WaxColor.FromRgb24(pImage[x, y]);
                var nearest = inputColor.Nearest(colors.Select(blockColor => blockColor.Color), Method);

                outImage[x, y] = nearest.ToRgb24();
                var info = colors.Find(blockColor => blockColor.Color.IsEquals(nearest)).Info;

                usedBlocks.Add(info);
                if (info.GeneratorProperties.NeedSupport) usedBlocks.Add(ColorPalette.PlaceholderBlock);
            }
        });
        
        outImage.Mutate(ctx => ctx.Resize(OutputSize.X, OutputSize.Y));

        var blockCount = new Dictionary<BlockInfo, int> { { ColorPalette.PlaceholderBlock, size.X } };
        foreach (var block in usedBlocks)
        {
            if (blockCount.ContainsKey(block)) blockCount[block]++;
            else blockCount.Add(block, 1);
        }
        

        return new PreviewOutput(outImage, blockCount.OrderByDescending(bc => bc.Value).ToDictionary());
    }

    public PreviewOutput GeneratePreviewFlat(Image<Rgb24> input)
    {
        var size = MapSize * 128;
        var usedBlocks = new ConcurrentBag<BlockInfo>();
        var outImage = new Image<Rgb24>(size.X, size.Y);
        var pImage = new ImageProcessor(size, Dithering).Process(input);

        var colors = new List<BlockColor>();
        foreach (var (_, info) in ColorPalette.Colors)
            colors.Add(new BlockColor(info.Color * MapColors.M1, info));

        Parallel.For(0, size.X, x =>
        {
            for (int y = 0; y < size.Y; y++)
            {
                var inputColor = WaxColor.FromRgb24(pImage[x, y]);
                var nearest = inputColor.Nearest(colors.Select(blockColor => blockColor.Color), Method);

                outImage[x, y] = nearest.ToRgb24();
                var info = colors.Find(blockColor => blockColor.Color.IsEquals(nearest)).Info;

                usedBlocks.Add(info);
            }
        });

        outImage.Mutate(ctx => ctx.Resize(OutputSize.X, OutputSize.Y));

        var blockCount = new Dictionary<BlockInfo, int>();

        foreach (var block in usedBlocks)
        {
            if (blockCount.ContainsKey(block)) blockCount[block]++;
            else blockCount.Add(block, 1);
        }

        return new PreviewOutput(outImage, blockCount.OrderByDescending(bc => bc.Value).ToDictionary());
    }
}

public record struct PreviewOutput(Image<Rgb24> Image, Dictionary<BlockInfo, int> BlockList);