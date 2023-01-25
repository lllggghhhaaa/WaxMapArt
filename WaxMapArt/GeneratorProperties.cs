using Newtonsoft.Json;

namespace WaxMapArt;

public struct GeneratorProperties
{
    [JsonProperty("need_support")] public bool NeedSupport;
}