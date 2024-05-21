using System.Collections.Concurrent;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using WaxMapArt.ImageProcessing;
using WaxMapArt.ImageProcessing.Dithering;

namespace WaxMapArt;

public class Preview(Palette colorPalette)
{
    public ComparisonMethod Method = ComparisonMethod.Cie76;
    public WaxSize MapSize = new(1, 1);
    public WaxSize OutputSize = new(128, 128);
    public DitheringType Dithering = DitheringType.None;

    public PreviewOutput GeneratePreviewStaircase(Image<Rgb24> input)
    {
        var size = MapSize * 128;
        var usedBlocks = new ConcurrentBag<BlockInfo>();
        var outImage = new Image<Rgb24>(size.X, size.Y);
        var pImage = new ImageProcessor(size).Process(input);

        var colors = new List<BlockColor>();
        
        foreach (var (_, info) in colorPalette.Colors)
        {
            var baseColor = info.Color;
            
            colors.Add(new BlockColor(baseColor * MapColors.M0, info));
            colors.Add(new BlockColor(baseColor * MapColors.M1, info));
            colors.Add(new BlockColor(baseColor, info));
        }
        
        List<WaxColor> ditherPalette = colors.Select(b => b.Color).ToList();
        IWaxDithering dithering = Dithering switch
        {
            DitheringType.None => new NoDithering(),
            DitheringType.FloydSteinberg => new FloydSteinbergDithering(),
            DitheringType.BayerOrdered4X4 => new BayerOrderedDithering(BayerOrderedDithering.Bayer4X4),
            DitheringType.BayerOrdered8X8 => new BayerOrderedDithering(BayerOrderedDithering.Bayer8X8),
            DitheringType.BayerOrdered16X16 => new BayerOrderedDithering(BayerOrderedDithering.Bayer16X16),
            _ => throw new ArgumentOutOfRangeException()
        };
        
        dithering.ApplyDither(ref pImage, ditherPalette, Method);

        Parallel.For(0, size.X, x =>
        {
            for (int y = 0; y < size.Y; y++)
            {
                var inputColor = WaxColor.FromRgb24(pImage[x, y]);
                var nearest = inputColor.Nearest(colors.Select(blockColor => blockColor.Color), Method);

                outImage[x, y] = nearest.ToRgb24();
                var info = colors.Find(blockColor => blockColor.Color.IsEquals(nearest)).Info;

                usedBlocks.Add(info);
                if (info.GeneratorProperties.NeedSupport) usedBlocks.Add(colorPalette.PlaceholderBlock);
            }
        });
        
        outImage.Mutate(ctx => ctx.Resize(OutputSize.X, OutputSize.Y));

        var blockCount = new Dictionary<BlockInfo, int> { { colorPalette.PlaceholderBlock, size.X } };
        foreach (var block in usedBlocks)
            if (!blockCount.TryAdd(block, 1)) blockCount[block]++;
        

        return new PreviewOutput(outImage, blockCount.OrderByDescending(bc => bc.Value).ToDictionary());
    }

    public PreviewOutput GeneratePreviewFlat(Image<Rgb24> input)
    {
        var size = MapSize * 128;
        var usedBlocks = new ConcurrentBag<BlockInfo>();
        var outImage = new Image<Rgb24>(size.X, size.Y);
        var pImage = new ImageProcessor(size).Process(input);

        var colors = new List<BlockColor>();
        foreach (var (_, info) in colorPalette.Colors)
            colors.Add(new BlockColor(info.Color * MapColors.M1, info));
        
        List<WaxColor> ditherPalette = colors.Select(b => b.Color).ToList();
        IWaxDithering dithering = Dithering switch
        {
            DitheringType.None => new NoDithering(),
            DitheringType.FloydSteinberg => new FloydSteinbergDithering(),
            DitheringType.BayerOrdered4X4 => new BayerOrderedDithering(BayerOrderedDithering.Bayer4X4),
            DitheringType.BayerOrdered8X8 => new BayerOrderedDithering(BayerOrderedDithering.Bayer8X8),
            DitheringType.BayerOrdered16X16 => new BayerOrderedDithering(BayerOrderedDithering.Bayer16X16),
            _ => throw new ArgumentOutOfRangeException()
        };
        
        dithering.ApplyDither(ref pImage, ditherPalette, Method);

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
            if (!blockCount.TryAdd(block, 1)) blockCount[block]++;

        return new PreviewOutput(outImage, blockCount.OrderByDescending(bc => bc.Value).ToDictionary());
    }
}

public record struct PreviewOutput(Image<Rgb24> Image, Dictionary<BlockInfo, int> BlockList);