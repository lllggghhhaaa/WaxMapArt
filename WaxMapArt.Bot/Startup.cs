using DSharpPlus;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Newtonsoft.Json;
using WaxMapArt.Bot.Commands;

namespace WaxMapArt.Bot;

public class Startup
{
    public DiscordShardedClient Client;
    public IReadOnlyDictionary<int, SlashCommandsExtension> SlashCommandsExtensions;
    public static IMongoDatabase Database;

    public async Task Run()
    {
        ConfigJson configJson = JsonConvert.DeserializeObject<ConfigJson>(await File.ReadAllTextAsync("config.json"))!;

        IMongoClient client = new MongoClient(configJson.MongoUri);
        Database = client.GetDatabase("Ciara");

        var cfg = new DiscordConfiguration
        {
            Token = configJson.Token,
            TokenType = TokenType.Bot,

            Intents = DiscordIntents.GuildMessages
                      | DiscordIntents.Guilds
                      | DiscordIntents.GuildMembers
                      | DiscordIntents.GuildBans
                      | DiscordIntents.GuildVoiceStates,

            AutoReconnect = true,
            MinimumLogLevel = LogLevel.Debug,
            LogTimestampFormat = "dd MMM yyy - hh:mm:ss"
        };

        Client = new DiscordShardedClient(cfg);

        await Client.UseInteractivityAsync(new InteractivityConfiguration
        {
            PollBehaviour = PollBehaviour.KeepEmojis,
            Timeout = TimeSpan.FromSeconds(30)
        });

        SlashCommandsExtensions = await Client.UseSlashCommandsAsync();

        // Register commands
        SlashCommandsExtensions.RegisterCommands<ArtCommands>();
        SlashCommandsExtensions.RegisterCommands<PaletteCommands>();

        Client.ComponentInteractionCreated += async (_, args) =>
        {
            await args.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
        };

        Client.ClientErrored += async (_, args) => { Client.Logger.LogError(args.Exception, "error"); };

        foreach (var (_, value) in SlashCommandsExtensions)
        {
            value.SlashCommandErrored += async (_, args) => { Client.Logger.LogError(args.Exception, "error"); };
        }

        await Client.StartAsync();
        await Task.Delay(-1);
    }
}