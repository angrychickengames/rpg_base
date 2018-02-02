using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterBuff
{
    public GamePlayManager Manager { get { return GamePlayManager.Singleton; } }
    public CharacterEntity Giver { get; private set; }
    public CharacterEntity Receiver { get; private set; }
    public Skill Skill { get; private set; }
    public int BuffIndex { get; private set; }
    public string Id { get { return Skill.Id + "_" + BuffIndex; } }
    public SkillBuff Buff { get { return Skill.buffs[BuffIndex]; } }
    private int healAmount;
    private int turnsCount;
    public int ApplyTurns { get { return Buff.applyTurns; } }
    public int TurnsCount { get { return turnsCount; } }
    public int RemainsTurns { get { return ApplyTurns - TurnsCount; } }
    private readonly List<GameEffect> effects = new List<GameEffect>();

    public CharacterBuff(CharacterEntity giver, CharacterEntity receiver, Skill skill, int buffIndex)
    {
        Giver = giver;
        Receiver = receiver;
        Skill = skill;
        BuffIndex = buffIndex;

        if (Buff.pAtkHealRate != 0)
            healAmount += Mathf.CeilToInt(giver.Item.Attributes.pAtk * Buff.pAtkHealRate);
        if (Buff.mAtkHealRate != 0)
            healAmount += Mathf.CeilToInt(giver.Item.Attributes.mAtk * Buff.mAtkHealRate);
        ApplyHeal();

        if (Buff.buffEffects != null)
            effects.AddRange(Buff.buffEffects.InstantiatesTo(receiver));
    }

    public void IncreaseTurnsCount()
    {
        if (IsEnd())
            return;

        ApplyHeal();
        ++turnsCount;
    }

    public void ApplyHeal()
    {
        if (Mathf.Abs(healAmount) <= 0)
            return;

        Receiver.Hp += healAmount;
        if (healAmount > 0)
            Manager.SpawnHealText(Mathf.Abs(healAmount), Receiver);
        else
            Manager.SpawnPoisonText(Mathf.Abs(healAmount), Receiver);
    }

    public bool IsEnd()
    {
        return TurnsCount >= ApplyTurns;
    }

    public void BuffRemove()
    {
        foreach (var effect in effects)
        {
            if (effect != null)
                effect.DestroyEffect();
        }
        effects.Clear();
    }
}
