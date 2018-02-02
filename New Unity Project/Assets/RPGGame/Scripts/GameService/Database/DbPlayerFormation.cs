using System.Collections.Generic;

public class DbPlayerFormation : IPlayerFormation
{
    [LiteDB.BsonId]
    public string Id { get; set; }
    public string PlayerId { get; set; }
    public string DataId { get; set; }
    public int Position { get; set; }
    public string ItemId { get; set; }

    public static List<PlayerFormation> CloneList(IEnumerable<DbPlayerFormation> list)
    {
        var result = new List<PlayerFormation>();
        foreach (var entry in list)
        {
            var newEntry = new PlayerFormation();
            PlayerFormation.CloneTo(entry, newEntry);
            result.Add(newEntry);
        }
        return result;
    }
}
