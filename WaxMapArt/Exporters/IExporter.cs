using WaxMapArt.Models;

namespace WaxMapArt.Exporters;

public interface IExporter
{
    Stream Export(IReadOnlyList<BlockInfo> blocks, Palette palette);
    string FileExtension { get; }
}
