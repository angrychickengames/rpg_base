using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LootBoxReward
{
    public ItemAmount rewardItem;
    public int randomWeight;
}

public enum LootBoxRequirementType : short
{
    RequireSoftCurrency = 0,
    RequireHardCurrency,
}

public class LootBox : BaseGameData
{
    public Sprite icon;
    public LootBoxRequirementType requirementType;
    public int requireCurrencyAmount;
    public LootBoxReward[] lootboxRewards;

    public LootBoxReward RandomReward()
    {
        var weight = new Dictionary<LootBoxReward, int>();
        foreach (var lootboxReward in lootboxRewards)
        {
            weight.Add(lootboxReward, lootboxReward.randomWeight);
        }
        return WeightedRandomizer.From(weight).TakeOne();
    }
}
