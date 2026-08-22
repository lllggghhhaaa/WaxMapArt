namespace WaxMapArt.Models;

public class Block
{
    public string Id { get; set; }
    public int ColorId { get; set; }
    
    public Dictionary<string, string>  Properties { get; set; } = new();
    public bool NeedSupport { get; set; } = false;
}