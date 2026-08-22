using WaxMapArt.Models;

namespace WaxMapArt.Generators;

public class FlatGenerator : IGenerator
{
    public BlockInfo[] Generate(PixelMatch[,] pixels, Palette palette)
    {
        var width = pixels.GetLength(0);
        var height = pixels.GetLength(1);
        var blocks = new List<BlockInfo>(width * height + width);

        for (var x = 0; x < width; x++)
        {
            blocks.Add(new BlockInfo(x, 0, 0, palette.SupportBlock.Id,
                palette.SupportBlock.Properties));

            for (var y = 0; y < height; y++)
            {
                var match = pixels[x, y];
                blocks.Add(new BlockInfo(x, 0, y + 1, match.Id, match.Properties));

                if (match.NeedSupport)
                {
                    blocks.Add(new BlockInfo(x, -1, y + 1, palette.SupportBlock.Id,
                        palette.SupportBlock.Properties));
                }
            }
        }

        return blocks.ToArray();
    }
}
