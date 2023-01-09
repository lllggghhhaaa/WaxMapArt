namespace WaxMapArt.Bot.Utils;

public static class StringExtensionMethods
{
    public static async Task<Stream> ToStreamAsync(this string text)
    {
        var stream = new MemoryStream();
        await using var writer = new StreamWriter(stream);
        await writer.WriteAsync(text);
        await writer.FlushAsync();
        stream.Position = 0;

        return stream;
    }
}