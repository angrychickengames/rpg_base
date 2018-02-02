using System.Collections.Generic;

public class DbPlayer : IPlayer
{
    [LiteDB.BsonId]
    public string Id { get; set; }
    public string ProfileName { get; set; }
    public string LoginToken { get; set; }
    public int Exp { get; set; }
    public string SelectedFormation { get; set; }

    public static List<Player> CloneList(IEnumerable<DbPlayer> list)
    {
        var result = new List<Player>();
        foreach (var entry in list)
        {
            var newEntry = new Player();
            Player.CloneTo(entry, newEntry);
            result.Add(newEntry);
        }
        return result;
    }
}
