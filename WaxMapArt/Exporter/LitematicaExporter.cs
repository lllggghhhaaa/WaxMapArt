﻿using WaxMapArt.Entities;
using WaxNBT;
using WaxNBT.Tags;

namespace WaxMapArt.Exporter;

public class LitematicaExporter : IExporter
{
    public Stream SaveAsStream(Palette palette, BlockInfo[] blocks)
    {
        var dimensions = CalculateDimensions(blocks);

        var metadata = CreateMetadata(dimensions, blocks);
        var blockStatePalette = CreateBlockStatePalette(palette);
        var blockStates = CreateBlockStates(blocks, dimensions, palette);

        var map = CreateMapCompound(dimensions, blockStatePalette, blockStates);
        var regions = new NbtCompound("Regions").Add(map);

        var file = new NbtFile();
        file.Root
            .Add(metadata)
            .Add(regions)
            .Add(new NbtInt("MinecraftDataVersion", 3955))
            .Add(new NbtInt("Version", 7))
            .Add(new NbtInt("SubVersion", 1));

        return file.Serialize();
    }

    private static (int width, int height, int depth) CalculateDimensions(BlockInfo[] blocks)
    {
        if (blocks.Length == 0) return (0, 0, 0);

        var (minX, maxX) = (blocks.Min(b => b.X), blocks.Max(b => b.X));
        var (minY, maxY) = (blocks.Min(b => b.Y), blocks.Max(b => b.Y));
        var (minZ, maxZ) = (blocks.Min(b => b.Z), blocks.Max(b => b.Z));

        return (maxX - minX + 1, maxY - minY + 1, maxZ - minZ + 1);
    }

    private static NbtCompound CreateMetadata((int width, int height, int depth) dimensions, BlockInfo[] blocks)
    {
        var enclosingSize = new NbtCompound("EnclosingSize")
            .Add(new NbtInt("x", dimensions.width))
            .Add(new NbtInt("y", dimensions.height))
            .Add(new NbtInt("z", dimensions.depth));

        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        return new NbtCompound("Metadata")
            .Add(new NbtString("Name", "WaxMapArt"))
            .Add(new NbtString("Description", "Generated by WaxMapArt"))
            .Add(new NbtString("Author", "WaxMapArt"))
            .Add(enclosingSize)
            .Add(new NbtInt("RegionCount", 1))
            .Add(new NbtLong("TimeCreated", timestamp))
            .Add(new NbtLong("TimeModified", timestamp))
            .Add(new NbtInt("TotalBlocks", blocks.Length))
            .Add(new NbtInt("TotalVolume", dimensions.width * dimensions.height * dimensions.depth));
    }

    private static NbtList CreateBlockStatePalette(Palette palette)
    {
        var blockStatePalette = new NbtList("BlockStatePalette") { new NbtCompound("0").Add(new NbtString("Name", "minecraft:air")) };
        foreach (var color in palette.Colors) blockStatePalette.Add(new NbtCompound(color.MapId.ToString()).Add(new NbtString("Name", color.Id)));
        return blockStatePalette;
    }

    private static long[] CreateBlockStates(BlockInfo[] blocks, (int width, int height, int depth) dimensions, Palette palette)
    {
        var blockStates = new long[dimensions.width * dimensions.height * dimensions.depth];
        var colors = palette.Colors.ToList();
        
        foreach (var block in blocks)
        {
            var index = block.X + dimensions.width * (block.Y + dimensions.height * block.Z);
            blockStates[index] = colors.FindIndex(color => color.Id == block.Id) + 1;
        }

        return blockStates;
    }

    private static NbtCompound CreateMapCompound((int width, int height, int depth) dimensions, NbtList blockStatePalette, long[] blockStates) =>
        new NbtCompound("Map")
            .Add(blockStatePalette)
            .Add(new NbtLongArray("BlockStates", blockStates))
            .Add(new NbtList("Entities"))
            .Add(new NbtList("TileEntities"))
            .Add(new NbtList("PendingBlockTicks"))
            .Add(new NbtList("PendingFluidTicks"))
            .Add(new NbtCompound("Position")
                .Add(new NbtInt("x", 0))
                .Add(new NbtInt("y", 0))
                .Add(new NbtInt("z", 0)))
            .Add(new NbtCompound("Size")
                .Add(new NbtInt("x", dimensions.width))
                .Add(new NbtInt("y", dimensions.height))
                .Add(new NbtInt("z", dimensions.depth)));
}