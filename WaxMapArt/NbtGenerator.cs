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

        var paletteTag = new NbtList<NbtCompound>("palette");
        
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

            paletteTag.Data.Add(blockInfo);
        }

        var blocksTag = new NbtList<NbtCompound>("blocks");
        foreach (Block block in blocks)
        {
            var blockTag = new NbtCompound();
            blockTag.Add(new NbtList<NbtInt>("pos")
            {
                Data = new List<NbtInt>
                {
                    new(block.X),
                    new(block.Y), 
                    new(block.Z)
                }
            });
            
            int index = Array.FindIndex(palette, info => info.MapId == block.Info.MapId);
            blockTag.Add(new NbtInt("state", index));

            blocksTag.Data.Add(blockTag);
        }

        var nbt = new NbtFile();
        
        
        nbt.Root.Add(new NbtString("author", "lllggghhhaaa"));
        nbt.Root.Add(new NbtInt("DataVersion", 2586));
        nbt.Root.Add(blocksTag);
        nbt.Root.Add(paletteTag);
        nbt.Root.Add(new NbtList<NbtInt>("size")
        {
            Data = new List<NbtInt>
            {
                new(size.Item1),
                new(size.Item2), 
                new(size.Item3)
            }
        });

        Stream stream = nbt.Serialize();

        var ms = new MemoryStream();
        var gz = new GZipStream(ms, CompressionMode.Compress, true);
        stream.CopyTo(gz);
        stream.Close();
        gz.Close();

        ms.Position = 0;
        return ms;
    }
}