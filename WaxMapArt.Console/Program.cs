using CommandLine;
using Newtonsoft.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using WaxMapArt;

Parser.Default.ParseArguments<CliOptions>(args).WithParsed(o =>
{
    Palette palette = JsonConvert.DeserializeObject<Palette>(File.ReadAllText(o.Palette));
    Image<Rgb24> image = Image.Load<Rgb24>(o.ImagePath);

    Generator generator = new Generator(palette)
    {
        Method = o.ColorComparisonMethod,
        Dithering = o.Dithering,
        MapSize = new WaxSize(o.Width, o.Height),
        OutputSize = new WaxSize(512, 512)
    };

    GeneratorOutput generatorOutput = o.Staircase ? generator.GenerateStaircase(image) : generator.GenerateFlat(image);

    generatorOutput.Image.SaveAsPng(o.PreviewPath);

    foreach (var (mapId, count) in generatorOutput.BlockList)
    {
        int packs = count / 64;
        int rem = count % 64;
        double shulkers = Math.Truncate(packs / 27f * 100) / 100;
        
        string packCount = packs > 0 ? $"{packs} packs + {rem}" : count.ToString();
        string id = palette.Colors[mapId.ToString()].BlockId;
        
        Console.WriteLine(id);
        Console.WriteLine($"Shulker box: {shulkers}");
        Console.WriteLine(packCount);
        Console.WriteLine();
    }

    Stream stream = NbtGenerator.Generate(generatorOutput.Blocks);
    using FileStream fs = File.Create(o.OutputPath);
    stream.CopyTo(fs);

    stream.Close();
    fs.Close();
});