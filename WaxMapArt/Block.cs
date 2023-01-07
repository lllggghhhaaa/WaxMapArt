namespace WaxMapArt;

public class Block
{
    public int X;
    public int Y;
    public int Z;

    public BlockInfo Info;
}

public static class BlockExtensionMethods
{
    public static Tuple<int, int, int> CalculateSize(this Block[] blocks)
    {
        int minX = blocks.MinBy(block => block.X)!.X;
        int minY = blocks.MinBy(block => block.Y)!.Y;
        int minZ = blocks.MinBy(block => block.Z)!.Z;
        int maxX = blocks.MaxBy(block => block.X)!.X;
        int maxY = blocks.MaxBy(block => block.Y)!.Y;
        int maxZ = blocks.MaxBy(block => block.Z)!.Z;
        
        return new Tuple<int, int, int>(maxX - minX + 1, maxY - minY + 1, maxZ - minZ + 1);
    }
}