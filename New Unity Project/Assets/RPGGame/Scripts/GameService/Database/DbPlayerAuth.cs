using System.Collections.Generic;

public class DbPlayerAuth : IPlayerAuth
{
    [LiteDB.BsonId]
    public string Id { get; set; }
    public string PlayerId { get; set; }
    public string Type { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }

    public static List<PlayerAuth> CloneList(IEnumerable<DbPlayerAuth> list)
    {
        var result = new List<PlayerAuth>();
        foreach (var entry in list)
        {
            var newEntry = new PlayerAuth();
            PlayerAuth.CloneTo(entry, newEntry);
            result.Add(newEntry);
        }
        return result;
    }
}
