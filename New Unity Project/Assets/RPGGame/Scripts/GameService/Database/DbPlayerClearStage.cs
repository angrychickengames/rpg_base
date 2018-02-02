using System.Collections.Generic;

public class DbPlayerClearStage : IPlayerClearStage
{
    [LiteDB.BsonId]
    public string Id { get; set; }
    public string PlayerId { get; set; }
    public string DataId { get; set; }
    public int BestRating { get; set; }

    public static List<PlayerClearStage> CloneList(IEnumerable<DbPlayerClearStage> list)
    {
        var result = new List<PlayerClearStage>();
        foreach (var entry in list)
        {
            var newEntry = new PlayerClearStage();
            PlayerClearStage.CloneTo(entry, newEntry);
            result.Add(newEntry);
        }
        return result;
    }
}
