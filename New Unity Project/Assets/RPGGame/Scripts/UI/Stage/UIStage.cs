using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class UIStage : UIDataItem<Stage>
{
    [Header("General")]
    public Text textTitle;
    public Text textDescription;
    public Text textStageNumber;
    public Image imageIcon;
    public UIStamina uiRequireStamina;
    public Text textRewardPlayerExp;
    public Text textRewardCharacterExp;
    public UIItemList uiRewardsItemList;
    public UIItemList uiEnemyItemList;
    [Header("Relates Elements")]
    public Button[] interactableButtonsWhenUnlocked;
    public GameObject[] activeObjectsWhenUnlocked;
    public GameObject[] inactiveObjectsWhenUnlocked;
    public UIStagePreparation uiStagePreparation;
    public UnityEvent eventSetStagePreparation;

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
        SetupInfo(data);
    }

    private void SetupInfo(Stage data)
    {
        if (textTitle != null)
            textTitle.text = data.title;

        if (textDescription != null)
            textDescription.text = data.description;

        if (textStageNumber != null)
            textStageNumber.text = data.stageNumber;

        if (imageIcon != null)
            imageIcon.sprite = data.icon;

        if (uiRequireStamina != null)
        {
            var staminaData = PlayerStamina.StageStamina.Clone().SetAmount(data.requireStamina, 0);
            uiRequireStamina.SetData(staminaData);
        }

        if (textRewardPlayerExp != null)
            textRewardPlayerExp.text = data.rewardPlayerExp.ToString("N0");

        if (textRewardCharacterExp != null)
            textRewardCharacterExp.text = data.rewardCharacterExp.ToString("N0");

        var sampleIdCount = 0;
        if (uiRewardsItemList != null)
        {
            var list = new List<PlayerItem>();
            var rewardItems = data.rewardItems;
            foreach (var rewardItem in rewardItems)
            {
                var item = rewardItem.item;
                var newEntry = new PlayerItem();
                newEntry.Id = (++sampleIdCount).ToString("N0");
                newEntry.DataId = item.Id;
                newEntry.Amount = 1;
                list.Add(newEntry);
            }
            uiRewardsItemList.SetListItems(list, (ui) => ui.displayStats = UIItem.DisplayStats.Hidden);
        }

        if (uiEnemyItemList != null)
        {
            var dict = new Dictionary<string, PlayerItem>();
            var randomFoes = data.randomFoes;
            foreach (var randomFoe in randomFoes)
            {
                foreach (var foe in randomFoe.foes)
                {
                    if (foe.character != null)
                    {
                        var newEntry = PlayerItem.CreateActorItemWithLevel(foe.character, foe.level);
                        newEntry.Id = foe.character.Id + "_" + foe.level;
                        dict[foe.character.Id + "_" + foe.level] = newEntry;
                    }
                }
            }
            var waves = data.waves;
            foreach (var wave in waves)
            {
                if (wave.useRandomFoes)
                    continue;

                var foes = wave.foes;
                foreach (var foe in foes)
                {
                    var item = foe.character;
                    if (item != null)
                    {
                        var newEntry = PlayerItem.CreateActorItemWithLevel(item, foe.level);
                        newEntry.Id = foe.character.Id + "_" + foe.level;
                        dict[foe.character.Id + "_" + foe.level] = newEntry;
                    }
                }
            }
            var list = new List<PlayerItem>(dict.Values);
            list.SortLevel();
            uiEnemyItemList.SetListItems(list, (ui) => ui.displayStats = UIItem.DisplayStats.Level);
        }

        UpdateElementsWhenUnlocked();
    }

    public void UpdateElementsWhenUnlocked()
    {
        var isUnlocked = PlayerClearStage.IsUnlock(data);
        foreach (var button in interactableButtonsWhenUnlocked)
        {
            button.interactable = isUnlocked;
        }
        foreach (var obj in activeObjectsWhenUnlocked)
        {
            obj.SetActive(isUnlocked);
        }
        foreach (var obj in inactiveObjectsWhenUnlocked)
        {
            obj.SetActive(!isUnlocked);
        }
    }

    public void SetStagePreparationData()
    {
        if (uiStagePreparation != null)
        {
            uiStagePreparation.data = data;
            eventSetStagePreparation.Invoke();
        }
    }

    public void OnClickStartStage()
    {
        GamePlayManager.StartStage(data);
    }
}
