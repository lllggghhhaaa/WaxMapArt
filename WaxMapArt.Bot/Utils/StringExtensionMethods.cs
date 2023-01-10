using System.Text;

namespace WaxMapArt.Bot.Utils;

public static class StringExtensionMethods
{
    public static Stream ToStream(this string text) => new MemoryStream(Encoding.Unicode.GetBytes(text));
}