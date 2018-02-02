using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SkillUsageScope
{
    Self,
    Ally,
    Enemy,
    All,
}

public enum AttackScope
{
    SelectedTarget,
    SelectedAndOneRandomTargets,
    SelectedAndTwoRandomTargets,
    SelectedAndThreeRandomTargets,
    OneRandomEnemy,
    TwoRandomEnemies,
    ThreeRandomEnemies,
    FourRandomEnemies,
    AllEnemies,
}

public enum BuffScope
{
    Self,
    SelectedTarget,
    SelectedAndOneRandomTargets,
    SelectedAndTwoRandomTargets,
    SelectedAndThreeRandomTargets,
    OneRandomAlly,
    TwoRandomAllies,
    ThreeRandomAllies,
    FourRandomAllies,
    AllAllies,
    OneRandomEnemy,
    TwoRandomEnemies,
    ThreeRandomEnemies,
    FourRandomEnemies,
    AllEnemies,
    All,
}

public enum BuffType
{
    Buff,
    Nerf,
}

[System.Serializable]
public struct SkillAttackDamage
{
    public int fixDamage;
    public float pAtkDamageRate;
    public float mAtkDamageRate;
    [Tooltip("This will devide with calculated damage to show damage number text")]
    public int hitCount;
}

[System.Serializable]
public class SkillAttack
{
    public AttackScope attackScope;
    public AttackAnimationData attackAnimation;
    [Tooltip("Skill damage formula = `a.fixDamage` + ((`a.pAtkDamageRate` * `a.pAtk`) - `b.pDef`) + ((`a.mAtkDamageRate` * `a.mAtk`) - `b.mDef`)")]
    public SkillAttackDamage attackDamage;
    
    public AttackAnimationData AttackAnimation
    {
        get
        {
            if (attackAnimation == null)
                return AttackAnimationData.GetEmptyData();
            return attackAnimation;
        }
    }

    public SkillAttack Clone()
    {
        var result = new SkillAttack();
        result.attackScope = attackScope;
        result.attackDamage = attackDamage;
        return result;
    }
}

[System.Serializable]
public class SkillBuff
{
    public BuffScope buffScope;
    public Sprite icon;
    public CharacterEffectData buffEffects;
    public BuffType type;
    [Range(0f, 1f)]
    public float applyChance;
    [Range(1, 10)]
    public int applyTurns = 1;
    [Tooltip("Amount of buffs that will be cleared randomly on target")]
    [Range(0, 100)]
    public int clearBuffs = 0;
    [Tooltip("Amount of nerfs that will be cleared randomly on target")]
    [Range(0, 100)]
    public int clearNerfs = 0;
    public bool isStun;
    public CalculationAttributes attributes;
    [Tooltip("This will multiply with pAtk to calculate heal amount, You can set this value to be negative to make it as poison")]
    public float pAtkHealRate = 0;
    [Tooltip("This will multiply with mAtk to calculate heal amount, You can set this value to be negative to make it as poison")]
    public float mAtkHealRate = 0;

    public bool RandomToApply()
    {
        return Random.Range(0f, 1f) < applyChance;
    }

    public SkillBuff Clone()
    {
        var result = new SkillBuff();
        result.buffScope = buffScope;
        result.icon = icon;
        result.buffEffects = buffEffects.Clone();
        result.type = type;
        result.applyChance = applyChance;
        result.applyTurns = applyTurns;
        result.clearBuffs = clearBuffs;
        result.clearNerfs = clearNerfs;
        result.isStun = isStun;
        result.attributes = attributes;
        result.pAtkHealRate = pAtkHealRate;
        result.mAtkHealRate = mAtkHealRate;
        return result;
    }
}

public class Skill : BaseGameData
{
    public SkillUsageScope usageScope;
    public Sprite icon;
    public int coolDownTurns;
    [Tooltip("Attack each hits, leave its length to 0 to not attack")]
    public SkillAttack[] attacks;
    [Tooltip("Buffs, leave its length to 0 to not apply buffs")]
    public SkillBuff[] buffs;
    public SkillCastAnimationData castAnimation;

    public SkillCastAnimationData CastAnimation
    {
        get
        {
            if (castAnimation == null)
                return SkillCastAnimationData.GetEmptyData();
            return castAnimation;
        }
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (attacks.Length > 0)
            usageScope = SkillUsageScope.Enemy;
    }
#endif
}
