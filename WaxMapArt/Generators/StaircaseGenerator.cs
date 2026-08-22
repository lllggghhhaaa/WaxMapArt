using WaxMapArt.Models;

namespace WaxMapArt.Generators;

public class StaircaseGenerator : IGenerator
{
    public BlockInfo[] Generate(PixelMatch[,] pixels, Palette palette)
    {
        var width = pixels.GetLength(0);
        var height = pixels.GetLength(1);
        var allBlocks = new List<BlockInfo>(width * height * 2);
        var minGlobalY = int.MaxValue;

        var columnHeights = new int[width];

        for (var x = 0; x < width; x++)
        {
            allBlocks.Add(new BlockInfo(x, 0, 0, palette.SupportBlock.Id,
                palette.SupportBlock.Properties));

            var currentY = 0;
            var rowBlocks = new List<BlockInfo>(height);

            for (var y = 0; y < height; y++)
            {
                var match = pixels[x, y];
                currentY += match.Shading - 1;

                rowBlocks.Add(new BlockInfo(x, currentY, y + 1, match.Id, match.Properties));

                if (match.NeedSupport)
                {
                    rowBlocks.Add(new BlockInfo(x, currentY - 1, y + 1,
                        palette.SupportBlock.Id, palette.SupportBlock.Properties));
                }
            }

            var minY = int.MaxValue;
            foreach (var b in rowBlocks)
                if (b.Y < minY) minY = b.Y;

            var normalizeY = -minY;
            foreach (var b in rowBlocks)
            {
                var normalized = new BlockInfo(b.X, b.Y + normalizeY, b.Z, b.Id, b.Properties);
                allBlocks.Add(normalized);
                if (normalized.Y < minGlobalY) minGlobalY = normalized.Y;
            }

            columnHeights[x] = normalizeY;
        }

        if (minGlobalY > 0) return allBlocks.ToArray();

        var finalBlocks = new BlockInfo[allBlocks.Count];
        for (var i = 0; i < allBlocks.Count; i++)
        {
            var b = allBlocks[i];
            finalBlocks[i] = new BlockInfo(b.X, b.Y - minGlobalY, b.Z, b.Id, b.Properties);
        }

        return finalBlocks;
    }
}
