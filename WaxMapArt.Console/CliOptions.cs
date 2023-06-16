using CommandLine;
using WaxMapArt;
using WaxMapArt.ImageProcessing.Dithering;

public class CliOptions
{
    [Option('i', "image", Required = true, HelpText = "Input image path")]
    public string ImagePath { get; set; }
    [Option('o', "output", Required = true, HelpText = "Output schematic path")]
    public string OutputPath { get; set; }
    [Option('p', "preview", Required = true, HelpText = "Preview image path")]
    public string PreviewPath { get; set; }
    [Option('c', "palette", Required = true)]
    public string Palette { get; set; }
    [Option('m', "method", Default = ComparisonMethod.Cie76)]
    public ComparisonMethod ColorComparisonMethod { get; set; }
    [Option('d', "dithering", Default = DitheringType.None)]
    public DitheringType Dithering { get; set; }
    [Option('s', "staircase", Default = true)]
    public bool Staircase { get; set;}
    [Option('w', "width", Default = 1)]
    public int Width { get; set; }
    [Option('h', "height", Default = 1)]
    public int Height { get; set; }
}