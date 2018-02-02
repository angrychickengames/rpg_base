using System.Collections.Generic;

public class DbPlayerItem : IPlayerItem
{
    [LiteDB.BsonId]
    public string Id { get; set; }
    public string PlayerId { get; set; }
    public string DataId { get; set; }
    public int Amount { get; set; }
    public int Exp { get; set; }
    public string EquipItemId { get; set; }
    public string EquipPosition { get; set; }

    public static List<PlayerItem> CloneList(IEnumerable<DbPlayerItem> list)
    {
        var result = new List<PlayerItem>();
        foreach (var entry in list)
        {
            var newEntry = new PlayerItem();
            PlayerItem.CloneTo(entry, newEntry);
            result.Add(newEntry);
        }
        return result;
    }
}
