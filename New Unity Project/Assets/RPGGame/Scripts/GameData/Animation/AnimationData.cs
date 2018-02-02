using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationData : ScriptableObject
{
    [Tooltip("0 = No animation")]
    [Range(0, 1000)]
    public int animationActionState = 0;
    public float animationDuration = 0;
}
