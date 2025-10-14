using System.Collections.Concurrent;
using SkiaSharp;
using WaxMapArt.Entities;
using WaxMapArt.Utils;

namespace WaxMapArt.Generator;

public class StaircaseGenerator : IGenerator
{
    public GeneratorOutput Generate(SKBitmap image, Palette palette)
    {
        var blocks = new ConcurrentBag<BlockInfo>();
        var colors = ColorUtils.GetPaletteBlocks(palette, true);
        
        Parallel.For(0, image.Width, x =>
        {
            var row = new List<BlockInfo>(129)
            {
                new()
                {
                    X = x,
                    Y = 0,
                    Z = 0,
                    Id = palette.PlaceholderColor.Id,
                    Properties = palette.PlaceholderColor.Properties
                }
            };

            var lastY = 0;
            
            for (var y = 1; y < image.Height + 1; y++)
            {
                var pixel = image.GetPixel(x, y - 1);

                var blockInfo = colors.First(info => info.Color.Equals(pixel));
                lastY += blockInfo.Shading - 1;
                
                row.Add(new BlockInfo
                {
                    X = x,
                    Y = lastY,
                    Z = y,
                    Id = blockInfo.Id,
                    Properties = blockInfo.Properties
                });
                
                

                if (blockInfo.GeneratorProperties.NeedSupport)
                    row.Add(new BlockInfo
                    {
                        X = x,
                        Y = lastY - 1,
                        Z = y,
                        Id = palette.PlaceholderColor.Id,
                        Properties = palette.PlaceholderColor.Properties
                    });
            }

            var minY = row.MinBy(block => block.Y).Y;
            foreach (var t in row)
            {
                var blockInfo = t;
                ref var element = ref blockInfo;
                element.Y -= minY;
            }

            
            foreach (var b in row) blocks.Add(b);
        });
        
        return new GeneratorOutput(blocks.ToArray());
    }
}