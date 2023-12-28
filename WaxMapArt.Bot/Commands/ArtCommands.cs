using System.Diagnostics;
using System.Text;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using WaxMapArt.Bot.Models;
using WaxMapArt.Bot.Utils;
using WaxMapArt.ImageProcessing.Dithering;

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
        ComparisonMethod cca = ComparisonMethod.Cie76,
        [Option("dithering", "The dithering method")]
        DitheringType dithering = DitheringType.None,
        [Option("method", "The generate method")]
        GenerateMethod method = GenerateMethod.Staircase)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

        Stream stream = await attachment.DownloadAsStreamAsync();

        User user = await User.GetFromDatabaseAsync(Startup.Database, ctx.User.Id.ToString());
        Palette? palette = user.GetPalette(paletteName);

        if (palette is null)
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                .WithContent($"This palette doesn't exists\n```yaml\n{user.ListPaletteNamesYaml(ctx.User.Username)}\n```"));
            
            return;
        }
        
        Preview preview = new Preview(palette.Value)
        {
            MapSize = new WaxSize((int) width, (int) height),
            Method = cca,
            OutputSize = new WaxSize(512, 512),
            Dithering = dithering
        };

        PreviewOutput output = method switch
        {
            GenerateMethod.Flat => preview.GeneratePreviewFlat(await Image.LoadAsync<Rgb24>(stream)),
            GenerateMethod.Staircase => preview.GeneratePreviewStaircase(await Image.LoadAsync<Rgb24>(stream)),
            _ => preview.GeneratePreviewStaircase(await Image.LoadAsync<Rgb24>(stream))
        };
        
        Stream outStream = await output.Image.SaveAsStreamAsync(new PngEncoder());
        
        StringBuilder sb = new StringBuilder("Blocks: \n");
        foreach (var (mapId, count) in output.BlockList)
        {
            int packs = count / 64;
            int rem = count % 64;
            double shulkers = Math.Truncate(packs / 27f * 100) / 100;

            string packCount = packs > 0 ? $"{packs} packs + {rem}" : count.ToString();
            string id = palette.Value.Colors[mapId.ToString()].BlockId;
            
            sb.Append($"  {id}: {count} ({packCount} blocks) ({shulkers}SB)\n");
        }
        
        stopwatch.Stop();

        string resume = $"Size: {width * 128}x{height * 128}\n" +
                        $"Comparison method: {cca}\n" +
                        $"Dithering: {dithering}\n" +
                        $"Method: {method}\n" +
                        $"Elapsed time: {stopwatch.Elapsed:m\\:ss\\.fff}\n" +
                        sb;

        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Done :relaxed:")
            .AddFiles(new Dictionary<string, Stream>
            {
                { "preview.png", outStream },
                { "resume.yaml", resume.ToStream() }
            }));
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
        ComparisonMethod cca = ComparisonMethod.Cie76,
        [Option("dithering", "The dithering method")]
        DitheringType dithering = DitheringType.None,
        [Option("method", "The generate method")]
        GenerateMethod method = GenerateMethod.Staircase)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
        
        Stream stream = await attachment.DownloadAsStreamAsync();

        User user = await User.GetFromDatabaseAsync(Startup.Database, ctx.User.Id.ToString());
        Palette? palette = user.GetPalette(paletteName);

        if (palette is null)
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                .WithContent($"This palette doesn't exists\n```yaml\n{user.ListPaletteNamesYaml(ctx.User.Username)}\n```"));
            
            return;
        }
        
        Generator generator = new Generator(palette.Value)
        {
            MapSize = new WaxSize((int) width, (int) height),
            Method = cca,
            OutputSize = new WaxSize(512, 512),
            Dithering = dithering
        };

        GeneratorOutput output = method switch
        {
            GenerateMethod.Flat => generator.GenerateFlat(await Image.LoadAsync<Rgb24>(stream)),
            GenerateMethod.Staircase => generator.GenerateStaircase(await Image.LoadAsync<Rgb24>(stream)),
            _ => generator.GenerateStaircase(await Image.LoadAsync<Rgb24>(stream))
        };
        
        Stream outNbtStream = NbtGenerator.Generate(output.Blocks);

        Stream outImgStream = await output.Image.SaveAsStreamAsync(new PngEncoder());
        
        StringBuilder sb = new StringBuilder("Blocks: \n");
        foreach (var (mapId, count) in output.BlockList)
        {
            int packs = count / 64;
            int rem = count % 64;
            double shulkers = Math.Truncate(packs / 27f * 100) / 100;

            string packCount = packs > 0 ? $"{packs} packs + {rem}" : count.ToString();
            string id = palette.Value.Colors[mapId.ToString()].BlockId
                .Replace("minecraft:", "");
            
            sb.Append($"  {id}: {count} ({packCount} blocks) ({shulkers}SB)\n");
        }
        
        stopwatch.Stop();

        string resume = $"Size: {width * 128}x{height * 128}\n" +
                        $"Comparison method: {cca}\n" +
                        $"Dithering: {dithering}\n" +
                        $"Method: {method}\n" +
                        $"Elapsed time: {stopwatch.Elapsed:m\\:ss\\.fff}\n" +
                        sb;

        await ctx.EditResponseAsync(new DiscordWebhookBuilder()
            .WithContent("Done :relaxed:")
            .AddFiles(new Dictionary<string, Stream>
            {
                { "preview.png", outImgStream },
                { "ceira.nbt", outNbtStream },
                { "resume.yaml", resume.ToStream() }
            }));
    }
}