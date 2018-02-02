using System.Collections.Generic;

public class DbPlayerUnlockItem : IPlayerUnlockItem
{
    [LiteDB.BsonId]
    public string Id { get; set; }
    public string PlayerId { get; set; }
    public string DataId { get; set; }
    public int Amount { get; set; }

    public static List<PlayerUnlockItem> CloneList(IEnumerable<DbPlayerUnlockItem> list)
    {
        var result = new List<PlayerUnlockItem>();
        foreach (var entry in list)
        {
            var newEntry = new PlayerUnlockItem();
            PlayerUnlockItem.CloneTo(entry, newEntry);
            result.Add(newEntry);
        }
        return result;
    }
}
