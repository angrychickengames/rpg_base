using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipmentItem : BaseActorItem
{
    [Header("Equipment Data")]
    public CalculationAttributes extraAttributes;
    public List<string> equippablePositions;
    public EquipmentItemEvolve evolveInfo;

    public override SpecificItemEvolve GetSpecificItemEvolve()
    {
        return evolveInfo;
    }
#if UNITY_EDITOR
    public override BaseActorItem CreateEvolveItemAsset(CreateEvolveItemData createEvolveItemData)
    {
        var newItem = ScriptableObjectUtility.CreateAsset<EquipmentItem>(name);
        newItem.extraAttributes = extraAttributes.Clone();
        newItem.equippablePositions = new List<string>(equippablePositions);
        newItem.evolveInfo = (EquipmentItemEvolve)evolveInfo.Clone();
        return newItem;
    }
#endif
}

[System.Serializable]
public class EquipmentItemAmount : BaseItemAmount<EquipmentItem> { }

[System.Serializable]
public class EquipmentItemDrop : BaseItemDrop<EquipmentItem> { }

[System.Serializable]
public class EquipmentItemEvolve : SpecificItemEvolve<EquipmentItem>
{
    public override SpecificItemEvolve<EquipmentItem> Create()
    {
        return new EquipmentItemEvolve();
    }
}
