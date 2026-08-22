using WaxMapArt.Models;

namespace WaxMapArt.Generators;

public interface IGenerator
{
    BlockInfo[] Generate(PixelMatch[,] pixels, Palette palette);
}
