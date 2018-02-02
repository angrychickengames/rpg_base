using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterItem : BaseActorItem
{
    [Header("Character Data")]
    public AttackAnimationData attackAnimation;
    public List<Skill> skills;
    public CharacterEntity model;
    public CharacterItemEvolve evolveInfo;

    public AttackAnimationData AttackAnimation
    {
        get
        {
            if (attackAnimation == null)
                return AttackAnimationData.GetEmptyData();
            return attackAnimation;
        }
    }

    public override SpecificItemEvolve GetSpecificItemEvolve()
    {
        return evolveInfo;
    }
#if UNITY_EDITOR
    public override BaseActorItem CreateEvolveItemAsset(CreateEvolveItemData createEvolveItemData)
    {
        var newItem = ScriptableObjectUtility.CreateAsset<CharacterItem>(name);
        newItem.skills = new List<Skill>(skills);
        newItem.model = model;
        newItem.evolveInfo = (CharacterItemEvolve)evolveInfo.Clone();
        newItem.attackAnimation = attackAnimation;
        return newItem;
    }
#endif
}

[System.Serializable]
public class CharacterItemAmount : BaseItemAmount<CharacterItem> { }

[System.Serializable]
public class CharacterItemDrop : BaseItemDrop<CharacterItem> { }

[System.Serializable]
public class CharacterItemEvolve : SpecificItemEvolve<CharacterItem>
{
    public override SpecificItemEvolve<CharacterItem> Create()
    {
        return new CharacterItemEvolve();
    }
}
