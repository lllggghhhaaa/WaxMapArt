namespace WaxMapArt.Models;

public class Palette(List<Block> blocks, Block supportBlock)
{
    public List<Block> Blocks = blocks;
    public Block SupportBlock = supportBlock;
}