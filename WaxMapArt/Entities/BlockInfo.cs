namespace WaxMapArt.Entities;

public record struct BlockInfo(
    int X, int Y, int Z, 
    string Id, 
    Dictionary<string, string> Properties);