using System.IO.Compression;
using WaxMapArt.Models;
using WaxNBT;
using WaxNBT.Tags;

namespace WaxMapArt.Exporters;

public class VanillaExporter : IExporter
{
    public string FileExtension => ".nbt";

    public Stream Export(IReadOnlyList<BlockInfo> blocks, Palette palette)
    {
        var dimensions = CalculateDimensions(blocks);
        var blockPalette = BuildPalette(blocks);
        var paletteLookup = BuildPaletteLookup(blockPalette);

        var paletteTag = new NbtList("palette");
        foreach (var (id, props) in blockPalette)
        {
            var entry = new NbtCompound()
                .Add(new NbtString("Name", id));

            if (props.Count > 0)
            {
                var propsTag = new NbtCompound("Properties");
                foreach (var (k, v) in props)
                    propsTag.Add(new NbtString(k, v));
                entry.Add(propsTag);
            }

            paletteTag.Add(entry);
        }

        var blocksTag = new NbtList("blocks");
        foreach (var block in blocks)
        {
            var stateIndex = paletteLookup[(block.Id, GetPropsKey(block.Properties))];

            blocksTag.Add(new NbtCompound()
                .Add(new NbtList("pos")
                {
                    new NbtInt(block.X),
                    new NbtInt(block.Y),
                    new NbtInt(block.Z)
                })
                .Add(new NbtInt("state", stateIndex)));
        }

        var file = new NbtFile();
        file.Root
            .Add(new NbtString("author", "WaxMapArt"))
            .Add(new NbtInt("DataVersion", 3955))
            .Add(blocksTag)
            .Add(paletteTag)
            .Add(new NbtList("size")
            {
                new NbtInt(dimensions.width),
                new NbtInt(dimensions.height),
                new NbtInt(dimensions.depth)
            });

        return Compress(file.SerializeToStream());
    }

    private static (int width, int height, int depth) CalculateDimensions(IReadOnlyList<BlockInfo> blocks)
    {
        if (blocks.Count == 0) return (0, 0, 0);

        var minX = int.MaxValue; var maxX = int.MinValue;
        var minY = int.MaxValue; var maxY = int.MinValue;
        var minZ = int.MaxValue; var maxZ = int.MinValue;

        foreach (var b in blocks)
        {
            if (b.X < minX) minX = b.X;
            if (b.X > maxX) maxX = b.X;
            if (b.Y < minY) minY = b.Y;
            if (b.Y > maxY) maxY = b.Y;
            if (b.Z < minZ) minZ = b.Z;
            if (b.Z > maxZ) maxZ = b.Z;
        }

        return (maxX - minX + 1, maxY - minY + 1, maxZ - minZ + 1);
    }

    private static List<(string Id, IReadOnlyDictionary<string, string> Properties)> BuildPalette(
        IReadOnlyList<BlockInfo> blocks)
    {
        var seen = new HashSet<string>();
        var palette = new List<(string, IReadOnlyDictionary<string, string>)>();

        foreach (var block in blocks)
        {
            var key = GetBlockKey(block.Id, block.Properties);
            if (seen.Add(key))
                palette.Add((block.Id, block.Properties));
        }

        return palette;
    }

    private static Dictionary<(string Id, string PropsKey), int> BuildPaletteLookup(
        List<(string Id, IReadOnlyDictionary<string, string> Properties)> palette)
    {
        var lookup = new Dictionary<(string, string), int>();
        for (var i = 0; i < palette.Count; i++)
            lookup[(palette[i].Id, GetPropsKey(palette[i].Properties))] = i;
        return lookup;
    }

    private static string GetBlockKey(string id, IReadOnlyDictionary<string, string> props) =>
        props.Count == 0 ? id : $"{id}[{GetPropsKey(props)}]";

    private static string GetPropsKey(IReadOnlyDictionary<string, string> props)
    {
        if (props.Count == 0) return string.Empty;
        var keys = props.Keys.OrderBy(k => k).ToArray();
        return string.Join(";", keys.Select(k => $"{k}={props[k]}"));
    }

    private static Stream Compress(Stream rawStream)
    {
        var ms = new MemoryStream();
        using var gzip = new GZipStream(ms, CompressionMode.Compress, true);
        rawStream.CopyTo(gzip);
        rawStream.Close();
        gzip.Flush();
        ms.Seek(0, SeekOrigin.Begin);
        return ms;
    }
}
