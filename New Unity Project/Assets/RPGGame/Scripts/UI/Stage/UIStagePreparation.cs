using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIStagePreparation : UIDataItem<Stage>
{
    public UIFormation uiCurrentFormation;
    public UIItem uiFormationSlotPrefab;
    public UIStage uiStage;
    public override void Clear()
    {
        // Don't clear
    }

    public override bool IsEmpty()
    {
        return data == null || string.IsNullOrEmpty(data.Id);
    }

    public override void UpdateData()
    {
        if (uiStage != null)
            uiStage.SetData(data);
    }

    public override void Show()
    {
        base.Show();
        if (uiCurrentFormation != null)
        {
            uiCurrentFormation.formationName = Player.CurrentPlayer.SelectedFormation;
            uiCurrentFormation.SetFormationData(uiFormationSlotPrefab);
        }
    }
}
