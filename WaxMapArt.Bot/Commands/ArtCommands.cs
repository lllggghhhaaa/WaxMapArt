using System.Diagnostics;
using System.Text;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Newtonsoft.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using WaxMapArt.Bot.Utils;

namespace WaxMapArt.Bot.Commands;

public class ArtCommands : ApplicationCommandModule
{
    [SlashCommand("preview", "Generate preview of the map")]
    public async Task Preview(InteractionContext ctx,
        [Option("image", "Image to transform")]
        DiscordAttachment attachment,
        [Option("palette", "Name of the palette")]
        string paletteName,
        [Option("width", "Numbers of map in horizontal")]
        long width = 1,
        [Option("height", "Numbers of map in horizontal")]
        long height = 1,
        [Option("color_comparator", "Color comparison algorithm")]
        ComparisonMethod cca = ComparisonMethod.Cie76)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

        Stream stream = await attachment.DownloadAsStreamAsync();

        Palette palette = JsonConvert.DeserializeObject<Palette>(await File.ReadAllTextAsync($"{paletteName}.json"));
        
        Preview preview = new Preview(palette)
        {
            MapSize = new WaxSize((int) width, (int) height),
            Method = cca,
            OutputSize = new WaxSize(512, 512)
        };

        PreviewOutput output = preview.GeneratePreview(await Image.LoadAsync<Rgb24>(stream));
        
        Stream outStream = await output.Image.SaveAsStreamAsync(new PngEncoder());
        
        StringBuilder sb = new StringBuilder("Blocks: \n");
        foreach (var (mapId, count) in output.BlockList)
        {
            int packs = count / 64;
            int rem = count % 64;
            double shulkers = Math.Truncate(packs / 27f * 100) / 100;

            string packCount = packs > 0 ? $"{packs} packs + {rem}" : count.ToString();
            string id = palette.Colors[mapId.ToString()].BlockId;
            
            sb.Append($"  {id}: {count} ({packCount} blocks) ({shulkers}SB)\n");
        }
        
        stopwatch.Stop();

        string resume = $"Size: {width * 128}x{height * 128}\n" +
                        $"Comparison method: {cca}\n" +
                        $"Elapsed time: {stopwatch.Elapsed:m\\:ss\\.fff}\n" +
                        sb;
        
        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Done :relaxed:\n```yaml\n{resume}```").AddFile("preview.png", outStream));
    }

    [SlashCommand("generate", "Generate the schematic of the map")]
    public async Task Generate(InteractionContext ctx,
        [Option("image", "Image to transform")]
        DiscordAttachment attachment,
        [Option("palette", "Name of the palette")]
        string paletteName,
        [Option("width", "Numbers of map in horizontal")]
        long width = 1,
        [Option("height", "Numbers of map in horizontal")]
        long height = 1,
        [Option("color_comparator", "Color comparison algorithm")]
        ComparisonMethod cca = ComparisonMethod.Cie76)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
        
        Stream stream = await attachment.DownloadAsStreamAsync();

        Palette palette = JsonConvert.DeserializeObject<Palette>(await File.ReadAllTextAsync($"{paletteName}.json"));
        
        Generator generator = new Generator(palette)
        {
            MapSize = new WaxSize((int) width, (int) height),
            Method = cca,
            OutputSize = new WaxSize(512, 512)
        };

        GeneratorOutput output = generator.Generate(await Image.LoadAsync<Rgb24>(stream));
        Stream outNbtStream = NbtGenerator.Generate(output.Blocks);

        Stream outImgStream = await output.Image.SaveAsStreamAsync(new PngEncoder());
        
        StringBuilder sb = new StringBuilder("Blocks: \n");
        foreach (var (mapId, count) in output.BlockList)
        {
            int packs = count / 64;
            int rem = count % 64;
            double shulkers = Math.Truncate(packs / 27f * 100) / 100;

            string packCount = packs > 0 ? $"{packs} packs + {rem}" : count.ToString();
            string id = palette.Colors[mapId.ToString()].BlockId;
            
            sb.Append($"  {id}: {count} ({packCount} blocks) ({shulkers}SB)\n");
        }
        
        stopwatch.Stop();

        string resume = $"Size: {width * 128}x{height * 128}\n" +
                        $"Comparison method: {cca}\n" +
                        $"Elapsed time: {stopwatch.Elapsed:m\\:ss\\.fff}\n" +
                        sb;

        await ctx.EditResponseAsync(new DiscordWebhookBuilder()
            .WithContent($"Done :relaxed:\n```yaml\n{resume}```")
            .AddFiles(new Dictionary<string, Stream>
            {
                { "preview.png", outImgStream },
                { "ceira.nbt", outNbtStream }
            }));
    }
}