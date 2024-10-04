using System.IO.Compression;
using WaxMapArt.Entities;
using WaxNBT;
using WaxNBT.Tags;

namespace WaxMapArt.Exporter;

public class VanillaExporter : IExporter
{
    public Stream SaveAsStream(Palette palette, BlockInfo[] blocks)
    {
        var colors = palette.Colors.ToList();   
        var dimensions = CalculateDimensions(blocks);

        var paletteTag = new NbtList("palette");
        
        foreach (var block in palette.Colors)
        {
            var blockInfo = new NbtCompound();
            blockInfo.Add(new NbtString("Name", block.Id));

            /*
            if (block.Properties.Count > 0)
            {
                var properties = new NbtCompound("Properties");
                foreach (var (name, value) in block.Properties) properties.Add(new NbtString(name, value));
                blockInfo.Add(properties);
            }
            */

            paletteTag.Add(blockInfo);
        }

        var blocksTag = new NbtList("blocks");
        foreach (var block in blocks)
        {
            var blockTag = new NbtCompound();
            blockTag.Add(new NbtList("pos")
            {
                new NbtInt(block.X),
                new NbtInt(block.Y),
                new NbtInt(block.Z)
            });

            var index = colors.FindIndex(color => color.Id == block.Id);
            blockTag.Add(new NbtInt("state", index));

            blocksTag.Add(blockTag);
        }

        var nbt = new NbtFile();
        
        nbt.Root.Add(new NbtString("author", "lllggghhhaaa"));
        nbt.Root.Add(new NbtInt("DataVersion", 2586));
        nbt.Root.Add(blocksTag);
        nbt.Root.Add(paletteTag);
        nbt.Root.Add(new NbtList("size")
        {
            new NbtInt(dimensions.width),
            new NbtInt(dimensions.height), 
            new NbtInt(dimensions.depth)
        });

        var stream = nbt.Serialize();

        var ms = new MemoryStream();
        var gz = new GZipStream(ms, CompressionMode.Compress, true);
        stream.CopyTo(gz);
        stream.Close();
        gz.Close();

        ms.Seek(0, SeekOrigin.Begin);
        return ms;
    }
    
    private static (int width, int height, int depth) CalculateDimensions(BlockInfo[] blocks)
    {
        if (blocks.Length == 0) return (0, 0, 0);

        var (minX, maxX) = (blocks.Min(b => b.X), blocks.Max(b => b.X));
        var (minY, maxY) = (blocks.Min(b => b.Y), blocks.Max(b => b.Y));
        var (minZ, maxZ) = (blocks.Min(b => b.Z), blocks.Max(b => b.Z));

        return (maxX - minX + 1, maxY - minY + 1, maxZ - minZ + 1);
    }
}