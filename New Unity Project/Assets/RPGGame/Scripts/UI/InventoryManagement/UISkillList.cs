using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class UISkillList : UIDataItemList<UISkill, Skill>
{
    public void SetListItems(List<Skill> list, UnityAction<UISkill> onSetListItem = null)
    {
        ClearListItems();
        foreach (var entry in list)
        {
            var ui = SetListItem(entry);
            if (ui != null && onSetListItem != null)
                onSetListItem(ui);
        }
    }

    public UISkill SetListItem(Skill data)
    {
        if (data == null || string.IsNullOrEmpty(data.Id))
            return null;
        var item = SetListItem(data.Id);
        item.SetData(data);
        return item;
    }
}
