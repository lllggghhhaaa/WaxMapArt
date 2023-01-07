using Newtonsoft.Json;
using WaxMapArt;

BlockInfo[] blocks = JsonConvert.DeserializeObject<BlockInfo[]>(File.ReadAllText("blocks.json"))!;

Palette palette = new Palette
{
    Colors = new Dictionary<int, BlockInfo>(),
    PlaceholderBlock = blocks[0]
};

foreach (IGrouping<int,BlockInfo> blockInfos in blocks.GroupBy(info => info.MapId))
    palette.Colors.Add(blockInfos.Key, blockInfos.First());
    
File.WriteAllText("ceira.json", JsonConvert.SerializeObject(palette));    