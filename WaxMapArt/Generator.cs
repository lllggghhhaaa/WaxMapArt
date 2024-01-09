using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Collections.Concurrent;
using System.Diagnostics;
using WaxMapArt.ImageProcessing;
using WaxMapArt.ImageProcessing.Dithering;

namespace WaxMapArt;

public class Generator
{
    public ComparisonMethod Method = ComparisonMethod.Cie76;
    public WaxSize MapSize = new(1, 1);
    public WaxSize OutputSize = new(128, 128);
    public DitheringType Dithering = DitheringType.None;
    public Palette ColorPalette;

    public Generator(Palette colorPalette) => ColorPalette = colorPalette;

    public GeneratorOutput GenerateStaircase(Image<Rgb24> input)
    {
        WaxSize size = MapSize * 128;
        var outImage = new Image<Rgb24>(size.X, size.Y);
        var pImage = new ImageProcessor(size, Dithering).Process(input);

        var colors = new List<BlockColor>();

        foreach (var (_, info) in ColorPalette.Colors)
        {
            Rgb24 baseColor = MapColors.BaseColors[info.MapId];

            colors.Add(new BlockColor(baseColor.Multiply(MapColors.M0), info));
            colors.Add(new BlockColor(baseColor.Multiply(MapColors.M1), info));
            colors.Add(new BlockColor(baseColor, info));
        }

        var blocks = new ConcurrentBag<Block>();

        Parallel.For(0, pImage.Width, x =>
        {
            Block[] row = new Block[pImage.Height + 1];
            var supports = new Dictionary<int, Block>();

            Tuple<int, Block>? previous = null;

            for (int y = pImage.Height; y > 0; y--)
            {
                Rgb24 inputColor = pImage[x, y - 1];
                Rgb24 nearest = inputColor.Nearest(colors.Select(blockColor => blockColor.Color), Method);

                outImage[x, y - 1] = nearest;
                int index = colors.FindIndex(blockColor => blockColor.Color == nearest);
                BlockInfo info = colors[index].Info;
                int shadow = index % 3;

                Block block = new Block
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
                    Info = ColorPalette.PlaceholderBlock,
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
                Info = ColorPalette.PlaceholderBlock
            };

            int minY = supports.Count <= 0
                ? row.MinBy(block => block.Y)!.Y
                : Math.Min(row.MinBy(block => block.Y)!.Y, supports.MinBy(pair => pair.Value.Y).Value.Y);

            for (int i = 0; i < row.Length; i++)
            {
                row[i].Y -= minY;

                if (supports.ContainsKey(i)) supports[i].Y -= minY;
            }

            foreach (var b in row) blocks.Add(b);
            foreach (var b in supports.Values) blocks.Add(b);
        });

        outImage.Mutate(ctx => ctx.Resize(OutputSize.X, OutputSize.Y));

        for (int i = 0; i < blocks.Count; i++)
        {
            if (blocks.ElementAt(i) is not null) continue;
            Debug.WriteLine(i);
            Debug.WriteLine($"X: {i % size.X} | Y: {i / size.X}");
        }

        return new GeneratorOutput(blocks.ToArray(), outImage);
    }

    public GeneratorOutput GenerateFlat(Image<Rgb24> input)
    {
        WaxSize size = MapSize * 128;
        var outImage = new Image<Rgb24>(size.X, size.Y);
        var pImage = new ImageProcessor(size, Dithering).Process(input);

        var colors = new List<BlockColor>();
        foreach (var (_, info) in ColorPalette.Colors)
            colors.Add(new BlockColor(MapColors.BaseColors[info.MapId].Multiply(MapColors.M1), info));

        var blocks = new ConcurrentBag<Block>();

        Parallel.For(0, pImage.Width, x =>
        {
            for (int y = 0; y < size.Y; y++)
            {
                var inputColor = pImage[x, y];
                var nearest = inputColor.Nearest(colors.Select(color => color.Color), Method);
                outImage[x, y] = nearest;

                var blockInfo = colors.Find(color => color.Color == nearest).Info;

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

public record struct GeneratorOutput(Block[] Blocks, Image<Rgb24> Image)
{
    public Dictionary<BlockInfo, int> CountBlocks()
    {
        var blockCount = new Dictionary<BlockInfo, int>();

        foreach (var block in Blocks) 
        {
            if (blockCount.ContainsKey(block.Info)) blockCount[block.Info]++;
            else blockCount.Add(block.Info, 1);
        }

        return blockCount.OrderByDescending(bc => bc.Value).ToDictionary();
    }
}