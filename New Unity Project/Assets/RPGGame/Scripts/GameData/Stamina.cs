using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum StaminaUnit
{
    Seconds,
    Minutes,
    Hours,
    Days,
}

[System.Serializable]
public class Stamina {
    public string id;
    public Sprite icon;
    public Int32Attribute maxAmountTable;
    public StaminaUnit recoverUnit;
    [Tooltip("Recover Duration, maximum is 360 days")]
    [Range(0, 360)]
    public int recoverDuration;
}
