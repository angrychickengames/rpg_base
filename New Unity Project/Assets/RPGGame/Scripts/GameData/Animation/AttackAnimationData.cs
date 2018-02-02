using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackAnimationData : AnimationData
{
    public float hitDuration = 0;
    public bool isRangeAttack;
    public Damage damage;

    private static AttackAnimationData emptyData;
    public static AttackAnimationData GetEmptyData()
    {
        if (emptyData == null)
            emptyData = CreateInstance<AttackAnimationData>();
        return emptyData;
    }
}
