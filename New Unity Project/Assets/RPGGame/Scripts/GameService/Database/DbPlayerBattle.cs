using System.Collections.Generic;

public class DbPlayerBattle : IPlayerBattle
{
    [LiteDB.BsonId]
    public string Id { get; set; }
    public string PlayerId { get; set; }
    public string DataId { get; set; }
    public string Session { get; set; }
    public uint BattleResult { get; set; }
    public int Rating { get; set; }

    public static List<PlayerBattle> CloneList(IEnumerable<DbPlayerBattle> list)
    {
        var result = new List<PlayerBattle>();
        foreach (var entry in list)
        {
            var newEntry = new PlayerBattle();
            PlayerBattle.CloneTo(entry, newEntry);
            result.Add(newEntry);
        }
        return result;
    }
}
