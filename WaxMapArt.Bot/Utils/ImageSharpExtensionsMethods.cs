using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;

namespace WaxMapArt.Bot.Utils;

public static class ImageSharpExtensionsMethods
{
    public static async Task<Stream> SaveAsStreamAsync(this Image image, IImageEncoder encoder)
    {
        Stream stream = new MemoryStream();
        await image.SaveAsync(stream, encoder);
        stream.Position = 0;

        return stream;
    }
}