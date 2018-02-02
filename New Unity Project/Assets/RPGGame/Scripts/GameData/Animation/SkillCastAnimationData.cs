using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillCastAnimationData : AnimationData
{
    public bool castAtMapCenter;
    public CharacterEffectData castEffects;

    private static SkillCastAnimationData emptyData;
    public static SkillCastAnimationData GetEmptyData()
    {
        if (emptyData == null)
            emptyData = CreateInstance<SkillCastAnimationData>();
        return emptyData;
    }
}
