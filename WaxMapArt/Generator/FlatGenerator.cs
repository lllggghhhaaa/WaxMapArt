using System.Collections.Concurrent;
using SkiaSharp;
using WaxMapArt.Entities;
using WaxMapArt.Utils;

namespace WaxMapArt.Generator;

public class FlatGenerator : IGenerator
{
    public GeneratorOutput Generate(SKBitmap image, SKBitmap originalImage, Palette palette)
    {
        var blocks = new ConcurrentBag<BlockInfo>();

        Parallel.For(0, image.Width, x =>
        {
            blocks.Add(new BlockInfo
            {
                X = x,
                Y = 0,
                Z = 0,
                Id = palette.PlaceholderColor.Id,
                Properties = palette.PlaceholderColor.Properties
            });
            
            for (var y = 1; y < image.Height + 1; y++)
            {
                var pixel = image.GetPixel(x, y - 1);

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