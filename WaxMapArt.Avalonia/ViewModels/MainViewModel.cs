using System;
using WaxMapArt.ImageProcessing.Dithering;
using Avalonia.Media;
using System.Collections.ObjectModel;

namespace WaxMapArt.Avalonia.ViewModels;

public class MainViewModel : ViewModelBase
{
    public decimal MapWidth { get; set; } = 1;
    public decimal MapHeight { get; set; } = 1;

    public int PaletteIndex { get; set; }
    public int ComparisonMethodIndex { get; set; }
    public int GenerateMethodIndex { get; set; }
    public int DitheringIndex { get; set; }

    public BlockInfo Block { get; set; } = new() { MapId = 4, BlockId = "Null", Color = new(35, 235, 153) };
    
    public WaxSize MapSize => new((int)MapWidth, (int)MapHeight);
    public ComparisonMethod Comparison => Enum.GetValues<ComparisonMethod>()[ComparisonMethodIndex];
    public GenerateMethod Generate => Enum.GetValues<GenerateMethod>()[GenerateMethodIndex];
    public DitheringType Dithering => Enum.GetValues<DitheringType>()[DitheringIndex];
}

public enum GenerateMethod
{
    Staircase,
    Flat
}