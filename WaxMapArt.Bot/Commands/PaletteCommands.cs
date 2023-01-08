using System.Text;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using Newtonsoft.Json;
using WaxMapArt.Bot.Models;

namespace WaxMapArt.Bot.Commands;

[SlashCommandGroup("pallet", "Commands to configure palettes")]
public class PaletteCommands : ApplicationCommandModule
{
    [SlashCommand("create", "Create a new palette")]
    public async Task Create(InteractionContext ctx,
        [Option("name", "Name of the palette")]
        string name,
        [Option("placeholder", "Placeholder block")]
        string phBlock)
    {
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
        BlockInfo[] blockList = JsonConvert.DeserializeObject<BlockInfo[]>(await File.ReadAllTextAsync("blocks.json"))!;

        var interactivity = ctx.Client.GetInteractivity();

        Palette palette = new Palette
        {
            Name = name,
            Colors = new Dictionary<string, BlockInfo>()
        };

        palette.PlaceholderBlock = blockList.First(info => info.BlockId == phBlock);

        var options = new List<DiscordSelectComponentOption>();

        foreach (IGrouping<int, BlockInfo> blockGroup in blockList.GroupBy(info => info.MapId))
        {
            int mapId = blockGroup.Key;

            options.Clear();
            foreach (BlockInfo block in blockGroup)
                options.Add(new DiscordSelectComponentOption(block.BlockId, block.BlockId));

            var message = await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Selecione o id {mapId}")
                .AddComponents(new DiscordSelectComponent(ctx.User.Id.ToString(), "Blocks", options)));

            var result = await interactivity.WaitForSelectAsync(message, ctx.User, ctx.User.Id.ToString());

            string selected = result.Result.Values.First();

            palette.Colors[mapId.ToString()] = blockList.First(info => info.BlockId == selected);
        }

        var stream = new MemoryStream();
        var writer = new StreamWriter(stream);
        await writer.WriteAsync(JsonConvert.SerializeObject(palette, Formatting.Indented));
        await writer.FlushAsync();
        stream.Position = 0;

        User user = await User.GetFromDatabaseAsync(Startup.Database, ctx.User.Id) ??
                    new User { UserId = ctx.User.Id.ToString() };

        user.Palettes.Add(palette);
        
        StringBuilder sb = new StringBuilder($"{ctx.User.Username}:");
        
        foreach (Palette userPalette in user.Palettes) sb.AppendLine($"  - {userPalette.Name}");

        await ctx.EditResponseAsync(new DiscordWebhookBuilder()
            .WithContent($"Done :relaxed:\n```yaml\n{sb}\n```")
            .AddFile($"{name}.json", stream));

        await user.SendToDatabaseAsync(Startup.Database);
    }

    [SlashCommand("upload", "Create a new palette")]
    public async Task Upload(InteractionContext ctx,
        [Option("palette", "The palette in the json format")]
        DiscordAttachment attachment)
    {
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
        
        HttpClient client = new HttpClient();
        string content = await (await client.GetAsync(attachment.Url)).Content.ReadAsStringAsync();

        Palette palette = JsonConvert.DeserializeObject<Palette>(content);

        User user = await User.GetFromDatabaseAsync(Startup.Database, ctx.User.Id) ??
                    new User { UserId = ctx.User.Id.ToString() };

        user.Palettes.Add(palette);

        StringBuilder sb = new StringBuilder($"{ctx.User.Username}:\n");
        
        foreach (Palette userPalette in user.Palettes) sb.AppendLine($"  - {userPalette.Name}");

        await ctx.EditResponseAsync(new DiscordWebhookBuilder()
            .WithContent($"Done :relaxed:\n```yaml\n{sb}\n```"));

        await user.SendToDatabaseAsync(Startup.Database);
    }
}