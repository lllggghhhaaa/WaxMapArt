using System.Collections.Concurrent;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using WaxMapArt.ImageProcessing;
using WaxMapArt.ImageProcessing.Dithering;

namespace WaxMapArt;

public class Generator(Palette colorPalette)
{
    public ComparisonMethod Method = ComparisonMethod.Cie76;
    public WaxSize MapSize = new(1, 1);
    public WaxSize OutputSize = new(128, 128);
    public DitheringType Dithering = DitheringType.None;

    public GeneratorOutput GenerateStaircase(Image<Rgb24> input)
    {
        WaxSize size = MapSize * 128;
        var outImage = new Image<Rgb24>(size.X, size.Y);
        var pImage = new ImageProcessor(size).Process(input);

        var colors = new List<BlockColor>();

        foreach (var (_, info) in colorPalette.Colors)
        {
            WaxColor baseColor = info.Color;

            colors.Add(new(baseColor * MapColors.M0, info));
            colors.Add(new(baseColor * MapColors.M1, info));
            colors.Add(new(baseColor, info));
        }
        
        List<WaxColor> ditherPalette = colors.Select(b => b.Color).ToList();
        IWaxDithering dithering = Dithering switch
        {
            DitheringType.None => new NoDithering(ditherPalette, Method),
            DitheringType.FloydSteinberg => new FloydSteinbergDithering(ditherPalette, Method),
            DitheringType.BayerOrdered4X4 => new BayerOrderedDithering(ditherPalette, Method, BayerOrderedDithering.Bayer4X4),
            DitheringType.BayerOrdered8X8 => new BayerOrderedDithering(ditherPalette, Method, BayerOrderedDithering.Bayer8X8),
            DitheringType.BayerOrdered16X16 => new BayerOrderedDithering(ditherPalette, Method, BayerOrderedDithering.Bayer16X16),
            _ => throw new ArgumentOutOfRangeException()
        };
        
        dithering.ApplyDither(ref pImage);

        var blocks = new ConcurrentBag<Block>();

        Parallel.For(0, pImage.Width, x =>
        {
            var row = new Block[pImage.Height + 1];
            var supports = new Dictionary<int, Block>();

            Tuple<int, Block>? previous = null;

            for (int y = pImage.Height; y > 0; y--)
            {
                var inputColor = WaxColor.FromRgb24(pImage[x, y - 1]);
                outImage[x, y - 1] = inputColor.ToRgb24();
                
                int index = colors.FindIndex(blockColor => blockColor.Color.IsEquals(inputColor));
                var info = colors[index].Info;
                int shadow = index % 3;

                var block = new Block
                {
                    X = x,
                    Z = y,
                    Info = info
                };

                if (previous is null) block.Y = 0;
                else if (previous.Item1 == 0) block.Y = previous.Item2.Y + 1;
                else if (previous.Item1 == 1) block.Y = previous.Item2.Y;
                else if (previous.Item1 == 2) block.Y = previous.Item2.Y - 1;
                previous = new Tuple<int, Block>(shadow, block);

                if (info.GeneratorProperties.NeedSupport) supports.Add(y, new Block
                {
                    Info = colorPalette.PlaceholderBlock,
                    X = x,
                    Y = block.Y - 1,
                    Z = y
                });

                row[y] = block;
            }

            int ly = 0;

            if (previous!.Item1 == 0) ly = previous.Item2.Y + 1;
            if (previous.Item1 == 1) ly = previous.Item2.Y;
            if (previous.Item1 == 2) ly = previous.Item2.Y - 1;

            row[0] = new Block
            {
                X = x,
                Y = ly,
                Z = 0,
                Info = colorPalette.PlaceholderBlock
            };

            int minY = supports.Count <= 0
                ? row.MinBy(block => block.Y)!.Y
                : Math.Min(row.MinBy(block => block.Y)!.Y, supports.MinBy(pair => pair.Value.Y).Value.Y);

            for (int i = 0; i < row.Length; i++)
            {
                row[i].Y -= minY;

                if (supports.TryGetValue(i, out var support)) support.Y -= minY;
            }

            foreach (var b in row) blocks.Add(b);
            foreach (var b in supports.Values) blocks.Add(b);
        });

        outImage.Mutate(ctx => ctx.Resize(OutputSize.X, OutputSize.Y));

        return new GeneratorOutput(blocks.ToArray(), outImage);
    }

    public GeneratorOutput GenerateFlat(Image<Rgb24> input)
    {
        WaxSize size = MapSize * 128;
        var outImage = new Image<Rgb24>(size.X, size.Y);
        var pImage = new ImageProcessor(size).Process(input);

        var colors = new List<BlockColor>();
        foreach (var (_, info) in colorPalette.Colors)
            colors.Add(new BlockColor(info.Color * MapColors.M1, info));

        List<WaxColor> ditherPalette = colors.Select(b => b.Color).ToList();
        IWaxDithering dithering = Dithering switch
        {
            DitheringType.None => new NoDithering(ditherPalette, Method),
            DitheringType.FloydSteinberg => new FloydSteinbergDithering(ditherPalette, Method),
            DitheringType.BayerOrdered4X4 => new BayerOrderedDithering(ditherPalette, Method, BayerOrderedDithering.Bayer4X4),
            DitheringType.BayerOrdered8X8 => new BayerOrderedDithering(ditherPalette, Method, BayerOrderedDithering.Bayer8X8),
            DitheringType.BayerOrdered16X16 => new BayerOrderedDithering(ditherPalette, Method, BayerOrderedDithering.Bayer16X16),
            _ => throw new ArgumentOutOfRangeException()
        };
        
        dithering.ApplyDither(ref pImage);
        
        var blocks = new ConcurrentBag<Block>();

        Parallel.For(0, pImage.Width, x =>
        {
            for (int y = 0; y < size.Y; y++)
            {
                var inputColor = WaxColor.FromRgb24(pImage[x, y]);
                outImage[x, y] = inputColor.ToRgb24();

                var blockInfo = colors.Find(color => color.Color.IsEquals(inputColor)).Info;

                blocks.Add(new Block
                {
                    X = x,
                    Y = 0,
                    Z = y,
                    Info = blockInfo
                });
            }
        });

        outImage.Mutate(ctx => ctx.Resize(OutputSize.X, OutputSize.Y));
        
        return new GeneratorOutput(blocks.ToArray(), outImage);
    }
}

public readonly record struct GeneratorOutput(Block[] Blocks, Image<Rgb24> Image)
{
    public Dictionary<BlockInfo, int> CountBlocks()
    {
        var blockCount = new Dictionary<BlockInfo, int>();

        foreach (var block in Blocks) 
        {
            if (!blockCount.TryAdd(block.Info, 1)) blockCount[block.Info]++;
        }

        return blockCount.OrderByDescending(bc => bc.Value).ToDictionary();
    }
}