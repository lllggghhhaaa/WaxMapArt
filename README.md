# WaxMapArt

![Render](./Assets/render.png)

>## Table of content
>- [Generating Preview](#rendering-preview-of-map)<br/>
>- [Generating Schematic](#generating-schematic-file--nbt-)
>- [Using the bot](#using-the-bot)
>  - [Generating Schematic](#generating-schematic)

**My bot is available in** <br/>
[![](https://dcbadge.vercel.app/api/server/MhCmnG6ZBA)](https://discord.gg/MhCmnG6ZBA)

### Rendering preview of map
```csharp
using Newtonsoft.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using WaxMapArt;

// Load the default palette.
Palette palette = JsonConvert.DeserializeObject<Palette>(File.ReadAllText("palette.json"));
// Load image from path.
Image<Rgb24> image = Image.Load<Rgb24>("image.jpg");

// Create the configuration to generate preview.
Preview preview = new Preview(palette)
{
    // Change the color comparison method.
    Method = ComparisonMethod.Cie76,
    // Amount of maps (X = width, Y = height).
    MapSize = new WaxSize(1, 1),
    // Change the output size of the preview image.
    OutputSize = new WaxSize(512, 512)
};

// Generate the preview.
PreviewOutput previewOutput = preview.GeneratePreviewStaircase(image); // Or preview.GeneratePreviewFlat(image) for flat generation

// Save the preview in a file.
previewOutput.Image.SaveAsPng("preview.png");

// Interact with all necessary blocks.
foreach (var (mapId, count) in previewOutput.BlockList)
{
    int packs = count / 64;
    int rem = count % 64;
    double shulkers = Math.Truncate(packs / 27f * 100) / 100;
    
    string packCount = packs > 0 ? $"{packs} packs + {rem}" : count.ToString();
    // Get the block id from the map id.
    string id = palette.Colors[mapId.ToString()].BlockId;
    
    // Print the result.
    Console.WriteLine(id);
    Console.WriteLine($"Shulker box: {shulkers}");
    Console.WriteLine(packCount);
    Console.WriteLine();
}
```

### Generating schematic file (NBT)

```csharp
using Newtonsoft.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using WaxMapArt;

// Load the default palette.
Palette palette = JsonConvert.DeserializeObject<Palette>(File.ReadAllText("palette.json"));
// Load image from path.
Image<Rgb24> image = Image.Load<Rgb24>("image.jpg");

// Create the configuration to generate blocks.
Generator generator = new Generator(palette)
{
    // Change the color comparison method.
    Method = ComparisonMethod.Cie76,
    // Amount of maps (X = width, Y = height).
    MapSize = new WaxSize(1, 1),
    // Change the output size of the preview image.
    OutputSize = new WaxSize(512, 512)
};

// Generate the list of blocks.
GeneratorOutput generatorOutput = generator.GenerateStaircase(image); // Or generator.GenerateFlat(image) for flat generation

// Save the preview in a file.
generatorOutput.Image.SaveAsPng("preview.png");

// Interact with all necessary blocks.
foreach (var (mapId, count) in generatorOutput.BlockList)
{
    int packs = count / 64;
    int rem = count % 64;
    double shulkers = Math.Truncate(packs / 27f * 100) / 100;
    
    string packCount = packs > 0 ? $"{packs} packs + {rem}" : count.ToString();
    // Get the block id from the map id.
    string id = palette.Colors[mapId.ToString()].BlockId;
    
    // Print the result.
    Console.WriteLine(id);
    Console.WriteLine($"Shulker box: {shulkers}");
    Console.WriteLine(packCount);
    Console.WriteLine();
}

// Finally, generate the NBT.
Stream stream = NbtGenerator.Generate(generatorOutput.Blocks);
// Save the NBT.
using FileStream fs = File.Create("schematic.nbt");
stream.CopyTo(fs);

stream.Close();
fs.Close();
```

## Using the bot

### Generating schematic
`/generate`

> #### Args
> `image` The image that will be transformed <br/>
> `palette` The name of the palette (default is **default**) <br/>
> `color_comparator` The color comparison algorithm (optional) (default is **CIE76**) <br/>
> `width` & `height` The number of maps (optional) (default is **1 x 1**) <br/>
> `dithering` The dithering algorithm (optional) (default is None) <br/>
> `method` The generation method (optional) (default is Staircase)

![generate](./Assets/Bot/generate.png)