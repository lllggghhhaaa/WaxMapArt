using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using Newtonsoft.Json;
using WaxMapArt.Bot.Models;
using WaxMapArt.Bot.Utils;

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

        var stream = await JsonConvert.SerializeObject(palette, Formatting.Indented).ToStreamAsync();
        
        User user = await User.GetFromDatabaseAsync(Startup.Database, ctx.User.Id.ToString());

        if (user.HasPalette(palette.Name))
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                .WithContent($"This palette already exists\n```yaml\n{user.ListPaletteNamesYaml(ctx.User.Username)}\n```"));
            
            return;
        }

        user.Palettes.Add(palette);
        
        await ctx.EditResponseAsync(new DiscordWebhookBuilder()
            .WithContent($"Done :relaxed:\n```yaml\n{user.ListPaletteNamesYaml(ctx.User.Username)}\n```")
            .AddFile($"{name}.json", stream));

        await user.SendToDatabaseAsync(Startup.Database);
    }

    [SlashCommand("upload", "Create a new palette")]
    public async Task Upload(InteractionContext ctx,
        [Option("palette", "The palette in the json format")]
        DiscordAttachment attachment)
    {
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

        string content = await attachment.DownloadAsStringAsync();

        Palette palette = JsonConvert.DeserializeObject<Palette>(content);

        User user = await User.GetFromDatabaseAsync(Startup.Database, ctx.User.Id.ToString());

        if (user.HasPalette(palette.Name))
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                .WithContent($"This palette already exists\n```yaml\n{user.ListPaletteNamesYaml(ctx.User.Username)}\n```"));
            
            return;
        }
        
        user.Palettes.Add(palette);

        await ctx.EditResponseAsync(new DiscordWebhookBuilder()
            .WithContent($"Done :relaxed:\n```yaml\n{user.ListPaletteNamesYaml(ctx.User.Username)}\n```"));

        await user.SendToDatabaseAsync(Startup.Database);
    }

    [SlashCommand("delete", "Delete an specified palette")]
    public async Task Delete(InteractionContext ctx,
        [Option("palette", "The palette in the json format")]
        string paletteName)
    {
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
        
        User user = await User.GetFromDatabaseAsync(Startup.Database, ctx.User.Id.ToString());
        Palette? palette = user.GetPalette(paletteName);

        if (palette is null)
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                .WithContent($"This palette doesn't exists\n```yaml\n{user.ListPaletteNamesYaml(ctx.User.Username)}\n```"));
            
            return;
        }

        user.Palettes.Remove(palette.Value);
        
        var stream = await JsonConvert.SerializeObject(palette, Formatting.Indented).ToStreamAsync();
        
        await ctx.EditResponseAsync(new DiscordWebhookBuilder()
            .WithContent("Done :relaxed:")
            .AddFile($"{palette.Value.Name}.json", stream));
    }

    [SlashCommand("view", "View an specified palette")]
    public async Task View(InteractionContext ctx,
        [Option("palette", "The palette in the json format")]
        string paletteName)
    {
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
        
        User user = await User.GetFromDatabaseAsync(Startup.Database, ctx.User.Id.ToString());
        Palette? palette = user.GetPalette(paletteName);
        
        if (palette is null)
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                .WithContent($"This palette doesn't exists\n```yaml\n{user.ListPaletteNamesYaml(ctx.User.Username)}\n```"));
            
            return;
        }

        var stream = await JsonConvert.SerializeObject(palette, Formatting.Indented).ToStreamAsync();
        
        await ctx.EditResponseAsync(new DiscordWebhookBuilder()
            .WithContent("Done :relaxed:")
            .AddFile($"{palette.Value.Name}.json", stream));
    }

    [SlashCommand("list", "List all palettes")]
    public async Task List(InteractionContext ctx)
    {
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
        
        User user = await User.GetFromDatabaseAsync(Startup.Database, ctx.User.Id.ToString());
        
        await ctx.EditResponseAsync(new DiscordWebhookBuilder()
            .WithContent($"Done :relaxed:\n```yaml\n{user.ListPaletteNamesYaml(ctx.User.Username)}\n```"));
    }
}