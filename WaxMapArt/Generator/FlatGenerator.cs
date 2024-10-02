using System.Collections.Concurrent;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using WaxMapArt.Entities;
using WaxMapArt.Utils;

namespace WaxMapArt.Generator;

public class FlatGenerator : IGenerator
{
    public GeneratorOutput Generate(Image<Rgb24> image, Palette palette)
    {
        var blocks = new ConcurrentBag<BlockInfo>();

        Parallel.For(0, image.Width, x =>
        {
            for (var y = 0; y < image.Height; y++)
            {
                var pixel = image[x, y];

                var blockInfo = palette.Colors.First(color => ColorUtils.MapIdToInfo(color.MapId).Color.Multiply(0.86d).Equals(pixel));
                
                blocks.Add(new BlockInfo
                {
                    X = x,
                    Y = 0,
                    Z = y,
                    Id = blockInfo.Id,
                    Properties = blockInfo.Properties
                });
            }
        });
        
        return new GeneratorOutput(blocks.ToArray());
    }
}