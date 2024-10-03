using System.Collections.Concurrent;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using WaxMapArt.Entities;
using WaxMapArt.Utils;

namespace WaxMapArt.Generator;

public class StaircaseGenerator : IGenerator
{
    public GeneratorOutput Generate(Image<Rgb24> image, Palette palette)
    {
        var blocks = new ConcurrentBag<BlockInfo>();
        var colors = ColorUtils.GetPaletteBlocks(palette, true);

        Parallel.For(0, image.Width, x =>
        {
            var row = new BlockInfo[image.Height + 1];
            
            row[0] = new BlockInfo
            {
                X = x,
                Y = 0,
                Z = 0,
                Id = palette.PlaceholderColor.Id,
                Properties = palette.PlaceholderColor.Properties
            };

            var lastY = 0;
            
            for (var y = 1; y < image.Height + 1; y++)
            {
                var pixel = image[x, y - 1];

                var blockInfo = colors.First(info => info.Color.Equals(pixel));
                lastY += blockInfo.Shading - 1;
                
                row[y] = new BlockInfo
                {
                    X = x,
                    Y = lastY,
                    Z = y,
                    Id = blockInfo.Name,
                    Properties = blockInfo.Properties
                };
            }

            var minY = row.MinBy(block => block.Y).Y;
            for (var i = 0; i < row.Length; i++) row[i].Y -= minY;
            
            foreach (var b in row) blocks.Add(b);
        });
        
        return new GeneratorOutput(blocks.ToArray());
    }
}