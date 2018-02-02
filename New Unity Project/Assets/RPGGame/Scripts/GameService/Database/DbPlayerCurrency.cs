using System.Collections.Generic;

public class DbPlayerCurrency : IPlayerCurrency
{
    [LiteDB.BsonId]
    public string Id { get; set; }
    public string PlayerId { get; set; }
    public string DataId { get; set; }
    public int Amount { get; set; }
    public int PurchasedAmount { get; set; }

    public static List<PlayerCurrency> CloneList(IEnumerable<DbPlayerCurrency> list)
    {
        var result = new List<PlayerCurrency>();
        foreach (var entry in list)
        {
            var newEntry = new PlayerCurrency();
            PlayerCurrency.CloneTo(entry, newEntry);
            result.Add(newEntry);
        }
        return result;
    }
}
