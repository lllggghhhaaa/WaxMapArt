using System.Collections.Generic;

namespace WaxMapArt.Models;

public readonly struct BlockInfo
{
    public int X { get; }
    public int Y { get; }
    public int Z { get; }
    public string Id { get; }
    public IReadOnlyDictionary<string, string> Properties { get; }

    public BlockInfo(int x, int y, int z, string id,
        IReadOnlyDictionary<string, string> properties)
    {
        X = x;
        Y = y;
        Z = z;
        Id = id;
        Properties = properties;
    }
}
