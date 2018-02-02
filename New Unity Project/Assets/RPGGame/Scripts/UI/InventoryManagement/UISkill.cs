using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UISkill : UIDataItem<Skill>
{
    public Text textTitle;
    public Text textDescription;
    public Image imageIcon;

    public override void Clear()
    {
        SetupInfo(null);
    }

    public override void UpdateData()
    {
        SetupInfo(data);
    }

    private void SetupInfo(Skill data)
    {
        if (textTitle != null)
            textTitle.text = data == null ? "" : data.title;

        if (textDescription != null)
            textDescription.text = data == null ? "" : data.description;

        if (imageIcon != null)
            imageIcon.sprite = data == null ? null : data.icon;
    }

    public override bool IsEmpty()
    {
        return data == null || string.IsNullOrEmpty(data.Id);
    }

    public void ShowDataOnMessageDialog()
    {
        GameInstance.Singleton.ShowMessageDialog(data.title, data.description);
    }
}
