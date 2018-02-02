using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(UIFollowWorldObject))]
public class UICharacterStats : UIBase
{
    public Text textHpPerMaxHp;
    public Text textHpPercent;
    public Image imageHpGage;
    public UILevel uiLevel;
    public UICharacterBuff[] uiBuffs;
    public CharacterEntity character;
    public GameObject selectableObject;
    public GameObject activatingObject;

    private UIFollowWorldObject tempObjectFollower;
    public UIFollowWorldObject TempObjectFollower
    {
        get
        {
            if (tempObjectFollower == null)
                tempObjectFollower = GetComponent<UIFollowWorldObject>();
            return tempObjectFollower;
        }
    }

    private void Update()
    {
        if (character == null)
            return;

        TempObjectFollower.targetObject = character.uiContainer;
        var rate = (float)character.Hp / (float)character.MaxHp;

        if (textHpPerMaxHp != null)
            textHpPerMaxHp.text = character.Hp.ToString("N0") + "/" + character.MaxHp.ToString("N0");

        if (textHpPercent != null)
            textHpPercent.text = (rate * 100).ToString("N2") + "%";

        if (imageHpGage != null)
            imageHpGage.fillAmount = rate;

        if (uiLevel != null)
        {
            uiLevel.level = character.Item.Level;
            uiLevel.maxLevel = character.Item.MaxLevel;
            uiLevel.collectExp = character.Item.CollectExp;
            uiLevel.nextExp = character.Item.NextExp;
        }

        if (selectableObject != null)
            selectableObject.SetActive(character.selectable);

        if (activatingObject != null)
            activatingObject.SetActive(character.IsActiveCharacter);

        var i = 0;
        var buffKeys = character.Buffs.Keys;
        foreach (var buffKey in buffKeys)
        {
            if (i >= uiBuffs.Length)
                break;
            var ui = uiBuffs[i];
            ui.buff = character.Buffs[buffKey];
            ui.Show();
            ++i;
        }
        for (; i < uiBuffs.Length; ++i)
        {
            var ui = uiBuffs[i];
            ui.Hide();
        }
    }
}
