using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class GameDatabase : ScriptableObject
{
    [Header("Player database")]
    [Range(1, 1000)]
    public int playerMaxLevel;
    [Tooltip("Requires Exp to levelup for each level")]
    public Int32Attribute playerExpTable;
    [Tooltip("`Soft Currency`, `Start Amount` is start amount when create new player")]
    public Currency softCurrency = new Currency() { id = "GOLD", startAmount = 0 };
    [Tooltip("`Hard Currency`, `Start Amount` is start amount when create new player")]
    public Currency hardCurrency = new Currency() { id = "GEM", startAmount = 0 };
    public Stamina stageStamina = new Stamina() { id = "STAGE_STAMINA", maxAmountTable = new Int32Attribute() };
    public List<Formation> stageFormations = new List<Formation>() {
        new Formation() { id = "STAGE_FORMATION_A" },
        new Formation() { id = "STAGE_FORMATION_B" },
        new Formation() { id = "STAGE_FORMATION_C" },
    };

    [Header("Item database")]
    [Tooltip("List of game items, place all items here (includes character, equipment)")]
    public List<BaseItem> items;

    [Header("Stage database")]
    [Tooltip("List of game stages, place all stages here")]
    public List<Stage> stages;

    [Header("Loot Box database")]
    [Tooltip("List of game loot boxes, place all loot boxes here")]
    public List<LootBox> lootBoxes;

    [Header("Game beginning")]
    [Tooltip("List of start items, place items that you want to give to players when begin the game")]
    public List<ItemAmount> startItems;
    [Tooltip("List of start characters, characters in this list will joined team formation when begin the game")]
    public List<CharacterItem> startCharacters;
    [Tooltip("List of stages that will be unlocked when begin the game")]
    public List<Stage> unlockStages;

    [Header("Gameplay")]
    [Tooltip("Base attributes for all characters while battle")]
    public CalculationAttributes characterBaseAttributes;
    [Tooltip("Price to revive all characters when all characters dies, this use hard currency")]
    public int revivePrice = 5;
    [Tooltip("This will caculate with sum Atk to random Atk as: Atk = Mathf.Random(Atk * minAtkVaryRate, Atk * maxAtkVaryRate)")]
    public float minAtkVaryRate;
    [Tooltip("This will caculate with sum Atk to random Atk as: Atk = Mathf.Random(Atk * minAtkVaryRate, Atk * maxAtkVaryRate)")]
    public float maxAtkVaryRate;
    [Tooltip("If this is true, system will reset item level to 1 when evolved")]
    public bool resetItemLevelAfterEvolve;

    public readonly Dictionary<string, BaseItem> Items = new Dictionary<string, BaseItem>();
    public readonly Dictionary<string, Currency> Currencies = new Dictionary<string, Currency>();
    public readonly Dictionary<string, Stamina> Staminas = new Dictionary<string, Stamina>();
    public readonly Dictionary<string, Formation> Formations = new Dictionary<string, Formation>();
    public readonly Dictionary<string, Stage> Stages = new Dictionary<string, Stage>();
    public readonly Dictionary<string, LootBox> LootBoxes = new Dictionary<string, LootBox>();

    public void Setup()
    {
        Items.Clear();
        Currencies.Clear();
        Staminas.Clear();
        Formations.Clear();
        Stages.Clear();
        LootBoxes.Clear();

        AddItemsToDatabase(items);

        var startItemList = new List<BaseItem>();
        foreach (var startItem in startItems)
        {
            startItemList.Add(startItem.item);
        }
        AddItemsToDatabase(startItemList);

        Currencies[softCurrency.id] = softCurrency;
        Currencies[hardCurrency.id] = hardCurrency;
        Staminas[stageStamina.id] = stageStamina;
        AddStageFormationsToDatabase(stageFormations);
        AddStagesToDatabase(stages);
        AddStagesToDatabase(unlockStages);
        AddLootBoxesToDatabase(lootBoxes);
    }

    private void AddItemsToDatabase(List<BaseItem> items)
    {
        foreach (var item in items)
        {
            var dataId = item.Id;
            if (!string.IsNullOrEmpty(dataId))
            {
                Items[dataId] = item;
            }
        }
    }

    private void AddStageFormationsToDatabase(List<Formation> formations)
    {
        foreach (var formation in formations)
        {
            var dataId = formation.id;
            if (!string.IsNullOrEmpty(dataId))
            {
                Formations[formation.id] = formation;
            }
        }
    }

    private void AddStagesToDatabase(List<Stage> stages)
    {
        foreach (var stage in stages)
        {
            var dataId = stage.Id;
            if (!string.IsNullOrEmpty(dataId))
            {
                Stages[stage.Id] = stage;
            }
        }
    }

    private void AddLootBoxesToDatabase(List<LootBox> lootBoxes)
    {
        foreach (var lootBox in lootBoxes)
        {
            var dataId = lootBox.Id;
            if (!string.IsNullOrEmpty(dataId))
            {
                LootBoxes[lootBox.Id] = lootBox;
            }
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        var validatedStageFormations = new List<Formation>();
        foreach (var stageFormation in stageFormations)
        {
            var containsId = false;
            foreach (var validatedStageFormation in validatedStageFormations)
            {
                if (stageFormation != null && validatedStageFormation.id == stageFormation.id)
                {
                    containsId = true;
                    break;
                }
            }
            if (!containsId && stageFormation != null && !validatedStageFormations.Contains(stageFormation))
                validatedStageFormations.Add(stageFormation);
        }
        stageFormations = validatedStageFormations;

        var validatedItemList = new List<BaseItem>();
        foreach (var item in items)
        {
            var containsId = false;
            foreach (var validatedItem in validatedItemList)
            {
                if (item != null && validatedItem.Id == item.Id)
                {
                    containsId = true;
                    break;
                }
            }
            if (!containsId && item != null && !validatedItemList.Contains(item))
                validatedItemList.Add(item);
        }
        items = validatedItemList;

        var validatedStageList = new List<Stage>();
        foreach (var stage in stages)
        {
            var containsId = false;
            foreach (var validatedStage in validatedStageList)
            {
                if (stage != null && validatedStage.Id == stage.Id)
                {
                    containsId = true;
                    break;
                }
            }
            if (!containsId && stage != null && !validatedStageList.Contains(stage))
                validatedStageList.Add(stage);
        }
        stages = validatedStageList;
        EditorUtility.SetDirty(this);
    }
#endif
}
