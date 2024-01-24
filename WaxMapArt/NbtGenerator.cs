using System.IO.Compression;
using WaxNBT;
using WaxNBT.Tags;

namespace WaxMapArt;

public static class NbtGenerator
{
    public static Stream Generate(Block[] blocks)
    {
        Tuple<int, int, int> size = blocks.CalculateSize();
        
        BlockInfo[] palette = blocks.GroupBy(block => block.Info.BlockId).Select(grouping => grouping.First().Info).ToArray();

        var paletteTag = new NbtList("palette");
        
        foreach (var block in palette)
        {
            var blockInfo = new NbtCompound();
            blockInfo.Add(new NbtString("Name", block.BlockId));

            if (block.Properties.Count > 0)
            {
                var properties = new NbtCompound("Properties");
                foreach (var (name, value) in block.Properties) properties.Add(new NbtString(name, value));
                blockInfo.Add(properties);
            }

            paletteTag.Add(blockInfo);
        }

        var blocksTag = new NbtList("blocks");
        foreach (Block block in blocks)
        {
            var blockTag = new NbtCompound();
            blockTag.Add(new NbtList("pos")
            {
                new NbtInt(block.X),
                new NbtInt(block.Y),
                new NbtInt(block.Z)
            });
            
            int index = Array.FindIndex(palette, info => info.MapId == block.Info.MapId);
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
            new NbtInt(size.Item1),
            new NbtInt(size.Item2), 
            new NbtInt(size.Item3)
        });

        Stream stream = nbt.Serialize();

        var ms = new MemoryStream();
        var gz = new GZipStream(ms, CompressionMode.Compress, true);
        stream.CopyTo(gz);
        stream.Close();
        gz.Close();

        ms.Seek(0, SeekOrigin.Begin);
        return ms;
    }
}