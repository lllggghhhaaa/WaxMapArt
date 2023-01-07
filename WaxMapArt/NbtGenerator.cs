using Cyotek.Data.Nbt;
using Cyotek.Data.Nbt.Serialization;

namespace WaxMapArt;

public static class NbtGenerator
{
    public static Stream Generate(Block[] blocks)
    {
        Tuple<int, int, int> size = blocks.CalculateSize();
        BlockInfo[] palette = blocks.GroupBy(block => block.Info.BlockId).Select(grouping => grouping.First().Info).ToArray();

        var paletteTag = new TagList("palette", TagType.Compound);
        
        for (int i = 0; i < palette.Length; i++)
        {
            var blockInfo = new TagCompound();
            blockInfo.Value.Add(new TagString("Name", palette[i].BlockId));

            if (palette[i].Properties.Count > 0)
            {
                var properties = new TagCompound("Properties");
                foreach (var (name, value) in palette[i].Properties) properties.Value.Add(new TagString(name, value));
                blockInfo.Value.Add(properties);
            }

            paletteTag.Value.Add(blockInfo);
        }

        var blocksTag = new TagList("blocks", TagType.Compound);
        foreach (Block block in blocks)
        {
            var blockTag = new TagCompound();
            blockTag.Value.Add(new TagList("pos", TagType.Int, new TagCollection { block.X, block.Y, block.Z }));
            blockTag.Value.Add(new TagInt("state", Array.IndexOf(palette, block.Info)));

            blocksTag.Value.Add(blockTag);
        }
        
        var document = new NbtDocument();
        TagCompound root = document.DocumentRoot;

        root.Value.Add(new TagString("author", "lllggghhhaaa"));
        root.Value.Add(new TagInt("DataVersion", 2586));
        root.Value.Add(blocksTag);
        root.Value.Add(paletteTag);
        root.Value.Add(new TagList("size", TagType.Int, new TagCollection { size.Item1, size.Item2, size.Item3 }));

        Stream stream = new MemoryStream();
        var writer = new BinaryTagWriter(stream);
        writer.WriteStartDocument();
        writer.WriteTag(document.DocumentRoot);
        writer.WriteEndDocument();

        stream.Position = 0;
        return stream;
    }
}