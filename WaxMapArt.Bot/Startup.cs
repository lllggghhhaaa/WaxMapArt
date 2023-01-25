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
    private DiscordShardedClient _client = null!;
    private IReadOnlyDictionary<int, SlashCommandsExtension> _slashCommandsExtensions = null!;
    public static IMongoDatabase Database = null!;

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

        _client = new DiscordShardedClient(cfg);

        await _client.UseInteractivityAsync(new InteractivityConfiguration
        {
            PollBehaviour = PollBehaviour.KeepEmojis,
            Timeout = TimeSpan.FromSeconds(30)
        });

        _slashCommandsExtensions = await _client.UseSlashCommandsAsync();

        // Register commands
        _slashCommandsExtensions.RegisterCommands<ArtCommands>();
        _slashCommandsExtensions.RegisterCommands<PaletteCommands>();

        _client.ComponentInteractionCreated += async (_, args) =>
        {
            await args.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
        };

        _client.ClientErrored += (_, args) =>
        {
            _client.Logger.LogError(args.Exception, "error");
            return Task.CompletedTask;
        };

        foreach (var (_, value) in _slashCommandsExtensions)
        {
            value.SlashCommandErrored += (_, args) =>
            {
                _client.Logger.LogError(args.Exception, "error");
                return Task.CompletedTask;
            };
        }

        await _client.StartAsync();
        await Task.Delay(-1);
    }
}