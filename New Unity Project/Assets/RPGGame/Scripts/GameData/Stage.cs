using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StageFoe
{
    public CharacterItem character;
    public int level;
}

[System.Serializable]
public class StageRandomFoe
{
    public StageFoe[] foes;
    public int randomWeight;
}

[System.Serializable]
public class StageWave
{
    public bool useRandomFoes;
    public StageFoe[] foes;
}

public class Stage : BaseGameData
{
    public string stageNumber;
    public Sprite icon;
    public EnvironmentData environment;
    [Header("Battle")]
    public StageWave[] waves;
    public StageRandomFoe[] randomFoes;
    [Header("Stamina")]
    public int requireStamina;
    [Header("Rewards")]
    public int randomSoftCurrencyMinAmount;
    public int randomSoftCurrencyMaxAmount;
    public int rewardPlayerExp;
    public int rewardCharacterExp;
    public ItemDrop[] rewardItems;
    [Header("Unlock")]
    public Stage[] unlockStages;

    public StageRandomFoe RandomFoes()
    {
        var weight = new Dictionary<StageRandomFoe, int>();
        foreach (var randomFoe in randomFoes)
        {
            weight.Add(randomFoe, randomFoe.randomWeight);
        }
        return WeightedRandomizer.From(weight).TakeOne();
    }
}
