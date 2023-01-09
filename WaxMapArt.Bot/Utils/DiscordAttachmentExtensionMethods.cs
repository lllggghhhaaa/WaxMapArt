using DSharpPlus.Entities;

namespace WaxMapArt.Bot.Utils;

public static class DiscordAttachmentExtensionMethods
{
    public static async Task<string> DownloadAsStringAsync(this DiscordAttachment attachment)
    {
        using var client = new HttpClient();
        return await (await client.GetAsync(attachment.Url)).Content.ReadAsStringAsync();
    }

    public static async Task<Stream> DownloadAsStreamAsync(this DiscordAttachment attachment)
    {
        using var client = new HttpClient();
        Stream stream = await (await client.GetAsync(attachment.Url)).Content.ReadAsStreamAsync();
        stream.Position = 0;

        return stream;
    }
}