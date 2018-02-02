using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UICharacterActionSkill : UICharacterAction
{
    public UISkill uiSkill;
    public Text textRemainsTurns;
    public Image imageRemainsTurnsGage;
    public int skillIndex;
    public CharacterSkill skill;

    private void Update()
    {
        if (skill == null)
            return;
        
        var rate = 1 - ((float)skill.TurnsCount / (float)skill.CoolDownTurns);

        if (uiSkill != null)
            uiSkill.data = skill.Skill;

        if (textRemainsTurns != null)
            textRemainsTurns.text = skill.RemainsTurns <= 0 ? "" : skill.RemainsTurns.ToString("N0");

        if (imageRemainsTurnsGage != null)
            imageRemainsTurnsGage.fillAmount = rate;
    }

    protected override void OnActionSelected()
    {
        ActionManager.ActiveCharacter.SetAction(skillIndex);
    }
}
