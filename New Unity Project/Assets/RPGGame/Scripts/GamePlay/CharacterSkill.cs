using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSkill
{
    public Skill Skill { get; private set; }
    private int turnsCount;
    public int CoolDownTurns { get { return Skill.coolDownTurns; } }
    public int TurnsCount { get { return turnsCount; } }
    public int RemainsTurns { get { return CoolDownTurns - TurnsCount; } }

    public CharacterSkill(Skill skill)
    {
        Skill = skill;
        turnsCount = skill.coolDownTurns;
    }

    public void IncreaseTurnsCount()
    {
        if (IsReady())
            return;

        ++turnsCount;
    }

    public bool IsReady()
    {
        return TurnsCount >= CoolDownTurns;
    }

    public void OnUseSkill()
    {
        turnsCount = 0;
    }
}
