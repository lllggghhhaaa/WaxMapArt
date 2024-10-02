using WaxMapArt.Entities;

namespace WaxMapArt.Exporter;

public interface IExporter
{
    public Stream SaveAsStream(Palette palette, BlockInfo[] blocks);
}