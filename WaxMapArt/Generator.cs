using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace WaxMapArt;

public class Generator
{
    public ComparisonMethod Method = ComparisonMethod.Cie76;
    public WaxSize MapSize = new(1, 1);
    public WaxSize OutputSize = new(128, 128);
    public Palette ColorPalette;

    public Generator(Palette colorPalette) => ColorPalette = colorPalette;

    public GeneratorOutput Generate(Image<Rgb24> input)
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

        var blocks = new List<Block>();

        for (int x = 0; x < input.Width; x++)
        {
            Block[] row = new Block[input.Height + 1]; 
            
            Tuple<int, Block>? previous = null;
            
            for (int y = input.Height; y > 0; y--)
            {
                Rgb24 inputColor = input[x, y - 1];
                Rgb24 nearest = inputColor.Nearest(colors.Select(blockColor => blockColor.Color), Method);

                outImage[x, y - 1] = nearest;
                int index = colors.FindIndex(blockColor => blockColor.Color == nearest);
                BlockInfo info = colors[index].Info;
                int shadow = index % 3;

                if (usedBlocks.ContainsKey(info.MapId))
                    usedBlocks[info.MapId]++;
                else
                    usedBlocks.Add(info.MapId, 1);

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

            int minY = row.MinBy(block => block.Y)!.Y;
            for (int i = 0; i < row.Length; i++)
            {
                Block block = row[i];
                block.Y -= minY;

                row[i] = block;
            }
            
            blocks.AddRange(row);
        }

        outImage.Mutate(ctx => ctx.Resize(OutputSize.X, OutputSize.Y));
        usedBlocks = new Dictionary<int, int>(usedBlocks.OrderByDescending(pair => pair.Value));
        usedBlocks.Add(ColorPalette.PlaceholderBlock.MapId, size.X);

        return new GeneratorOutput(blocks.ToArray(), outImage, usedBlocks);
    }
}

public record struct GeneratorOutput(Block[] Blocks, Image<Rgb24> Image, Dictionary<int, int> BlockList);