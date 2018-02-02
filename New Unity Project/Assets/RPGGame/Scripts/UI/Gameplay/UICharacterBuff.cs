using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UICharacterBuff : UIBase
{
    public Text textRemainsTurns;
    public Image imageRemainsTurnsGage;
    public Image imageIcon;
    public CharacterBuff buff;

    private void Update()
    {
        if (buff == null)
            return;

        var rate = (float)buff.TurnsCount / (float)buff.ApplyTurns;

        if (imageIcon != null)
            imageIcon.sprite = buff.Buff.icon;

        if (textRemainsTurns != null)
            textRemainsTurns.text = buff.RemainsTurns <= 0 ? "" : buff.RemainsTurns.ToString("N2");

        if (imageRemainsTurnsGage != null)
            imageRemainsTurnsGage.fillAmount = rate;
    }
}
