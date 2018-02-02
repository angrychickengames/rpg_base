using System.Collections.Generic;

public class DbPlayerStamina : IPlayerStamina
{
    [LiteDB.BsonId]
    public string Id { get; set; }
    public string PlayerId { get; set; }
    public string DataId { get; set; }
    public int Amount { get; set; }
    public long RecoveredTime { get; set; }

    public static List<PlayerStamina> CloneList(IEnumerable<DbPlayerStamina> list)
    {
        var result = new List<PlayerStamina>();
        foreach (var entry in list)
        {
            var newEntry = new PlayerStamina();
            PlayerStamina.CloneTo(entry, newEntry);
            result.Add(newEntry);
        }
        return result;
    }
}
