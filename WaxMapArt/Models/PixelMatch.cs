using System.Collections.Generic;

namespace WaxMapArt.Models;

public readonly struct PixelMatch
{
    public string Id { get; }
    public IReadOnlyDictionary<string, string> Properties { get; }
    public bool NeedSupport { get; }
    public byte Shading { get; }

    public PixelMatch(string id, IReadOnlyDictionary<string, string> properties,
        bool needSupport, byte shading)
    {
        Id = id;
        Properties = properties;
        NeedSupport = needSupport;
        Shading = shading;
    }
}
