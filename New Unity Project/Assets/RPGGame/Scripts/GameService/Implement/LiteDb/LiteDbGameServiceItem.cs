using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

public partial class LiteDbGameService
{
    protected override void DoLevelUpItem(string playerId, string loginToken, string itemId, Dictionary<string, int> materials, UnityAction<ItemResult> onFinish)
    {
        var result = new ItemResult();
        var foundPlayer = colPlayer.FindOne(a => a.Id == playerId && a.LoginToken == loginToken);
        var foundItem = colPlayerItem.FindOne(a => a.Id == itemId && a.PlayerId == playerId);
        if (foundPlayer == null)
            result.error = GameServiceErrorCode.INVALID_LOGIN_TOKEN;
        else if (foundItem == null)
            result.error = GameServiceErrorCode.INVALID_PLAYER_ITEM_DATA;
        else
        {
            var item = new PlayerItem();
            PlayerItem.CloneTo(foundItem, item);
            var softCurrency = GetCurrency(playerId, GameInstance.GameDatabase.softCurrency.id);
            var levelUpPrice = item.LevelUpPrice;
            var requireCurrency = 0;
            var increasingExp = 0;
            var updateItems = new List<PlayerItem>();
            var deleteItemIds = new List<string>();
            var materialItemIds = materials.Keys;
            var materialItems = new List<PlayerItem>();
            foreach (var materialItemId in materialItemIds)
            {
                var foundMaterial = colPlayerItem.FindOne(a => a.Id == materialItemId && a.PlayerId == playerId);
                if (foundMaterial == null)
                    continue;

                var resultItem = new PlayerItem();
                PlayerItem.CloneTo(foundMaterial, resultItem);
                if (resultItem.CanBeMaterial)
                    materialItems.Add(resultItem);
            }
            foreach (var materialItem in materialItems)
            {
                var usingAmount = materials[materialItem.Id];
                if (usingAmount > materialItem.Amount)
                    usingAmount = materialItem.Amount;
                requireCurrency += levelUpPrice * usingAmount;
                increasingExp += materialItem.RewardExp * usingAmount;
                materialItem.Amount -= usingAmount;
                if (materialItem.Amount > 0)
                    updateItems.Add(materialItem);
                else
                    deleteItemIds.Add(materialItem.Id);
            }
            if (requireCurrency > softCurrency.Amount)
                result.error = GameServiceErrorCode.NOT_ENOUGH_SOFT_CURRENCY;
            else
            {
                softCurrency.Amount -= requireCurrency;
                colPlayer.Update(foundPlayer);
                colPlayerCurrency.Update(softCurrency);
                var levelUpItem = new DbPlayerItem();
                item = item.CreateLevelUpItem(increasingExp);
                PlayerItem.CloneTo(item, levelUpItem);
                colPlayerItem.Update(levelUpItem);
                foreach (var deleteItemId in deleteItemIds)
                {
                    colPlayerItem.Delete(deleteItemId);
                }
                var resultSoftCurrency = new PlayerCurrency();
                PlayerCurrency.CloneTo(softCurrency, resultSoftCurrency);
                result.updateCurrencies.Add(resultSoftCurrency);
                result.updateItems = updateItems;
                result.updateItems.Add(item);
                result.deleteItemIds = deleteItemIds;
            }
        }
        onFinish(result);
    }

    protected override void DoEvolveItem(string playerId, string loginToken, string itemId, Dictionary<string, int> materials, UnityAction<ItemResult> onFinish)
    {
        var result = new ItemResult();
        var foundPlayer = colPlayer.FindOne(a => a.Id == playerId && a.LoginToken == loginToken);
        var foundItem = colPlayerItem.FindOne(a => a.Id == itemId && a.PlayerId == playerId);
        if (foundPlayer == null)
            result.error = GameServiceErrorCode.INVALID_LOGIN_TOKEN;
        else if (foundItem == null)
            result.error = GameServiceErrorCode.INVALID_PLAYER_ITEM_DATA;
        else
        {
            var item = new PlayerItem();
            PlayerItem.CloneTo(foundItem, item);

            if (!item.CanEvolve)
                result.error = GameServiceErrorCode.CANNOT_EVOLVE;
            else
            {
                var softCurrency = GetCurrency(playerId, GameInstance.GameDatabase.softCurrency.id);
                var requireCurrency = 0;
                var itemData = item.ItemData;
                requireCurrency = item.EvolvePrice;
                var enoughMaterials = true;
                var updateItems = new List<PlayerItem>();
                var deleteItemIds = new List<string>();
                var requiredMaterials = item.EvolveMaterials;   // This is Key-Value Pair for `playerItem.DataId`, `Required Amount`
                var materialItemIds = materials.Keys;
                var materialItems = new List<PlayerItem>();
                foreach (var materialItemId in materialItemIds)
                {
                    var foundMaterial = colPlayerItem.FindOne(a => a.Id == materialItemId && a.PlayerId == playerId);
                    if (foundMaterial == null)
                        continue;

                    var resultItem = new PlayerItem();
                    PlayerItem.CloneTo(foundMaterial, resultItem);
                    if (resultItem.CanBeMaterial)
                        materialItems.Add(resultItem);
                }
                foreach (var requiredMaterial in requiredMaterials)
                {
                    var dataId = requiredMaterial.Key;
                    var amount = requiredMaterial.Value;
                    foreach (var materialItem in materialItems)
                    {
                        if (materialItem.DataId != dataId)
                            continue;
                        var usingAmount = materials[materialItem.Id];
                        if (usingAmount > materialItem.Amount)
                            usingAmount = materialItem.Amount;
                        if (usingAmount > amount)
                            usingAmount = amount;
                        materialItem.Amount -= usingAmount;
                        amount -= usingAmount;
                        if (materialItem.Amount > 0)
                            updateItems.Add(materialItem);
                        else
                            deleteItemIds.Add(materialItem.Id);
                        if (amount == 0)
                            break;
                    }
                    if (amount > 0)
                    {
                        enoughMaterials = false;
                        break;
                    }
                }
                if (requireCurrency > softCurrency.Amount)
                    result.error = GameServiceErrorCode.NOT_ENOUGH_SOFT_CURRENCY;
                else if (!enoughMaterials)
                    result.error = GameServiceErrorCode.NOT_ENOUGH_ITEMS;
                else
                {
                    softCurrency.Amount -= requireCurrency;
                    colPlayer.Update(foundPlayer);
                    colPlayerCurrency.Update(softCurrency);
                    var evolvedItem = new DbPlayerItem();
                    item = item.CreateEvolveItem();
                    PlayerItem.CloneTo(item, evolvedItem);
                    colPlayerItem.Update(evolvedItem);
                    foreach (var updateEntry in updateItems)
                    {
                        var updateItem = new DbPlayerItem();
                        PlayerItem.CloneTo(updateEntry, updateItem);
                        colPlayerItem.Update(updateItem);
                    }
                    foreach (var deleteItemId in deleteItemIds)
                    {
                        colPlayerItem.Delete(deleteItemId);
                    }
                    var resultSoftCurrency = new PlayerCurrency();
                    PlayerCurrency.CloneTo(softCurrency, resultSoftCurrency);
                    result.updateCurrencies.Add(resultSoftCurrency);
                    result.updateItems = updateItems;
                    result.updateItems.Add(item);
                    result.deleteItemIds = deleteItemIds;
                }
            }
        }
        onFinish(result);
    }

    protected override void DoSellItems(string playerId, string loginToken, Dictionary<string, int> items, UnityAction<ItemResult> onFinish)
    {
        var result = new ItemResult();
        var player = colPlayer.FindOne(a => a.Id == playerId && a.LoginToken == loginToken);
        if (player == null)
            result.error = GameServiceErrorCode.INVALID_LOGIN_TOKEN;
        else
        {
            var softCurrency = GetCurrency(playerId, GameInstance.GameDatabase.softCurrency.id);
            var returnCurrency = 0;
            var updateItems = new List<PlayerItem>();
            var deleteItemIds = new List<string>();
            var sellingItemIds = items.Keys;
            var sellingItems = new List<PlayerItem>();
            foreach (var sellingItemId in sellingItemIds)
            {
                var foundItem = colPlayerItem.FindOne(a => a.Id == sellingItemId && a.PlayerId == playerId);
                if (foundItem == null)
                    continue;

                var resultItem = new PlayerItem();
                PlayerItem.CloneTo(foundItem, resultItem);
                if (resultItem.CanSell)
                    sellingItems.Add(resultItem);
            }
            foreach (var sellingItem in sellingItems)
            {
                var usingAmount = items[sellingItem.Id];
                if (usingAmount > sellingItem.Amount)
                    usingAmount = sellingItem.Amount;
                returnCurrency += sellingItem.SellPrice * usingAmount;
                sellingItem.Amount -= usingAmount;
                if (sellingItem.Amount > 0)
                    updateItems.Add(sellingItem);
                else
                    deleteItemIds.Add(sellingItem.Id);
            }
            softCurrency.Amount += returnCurrency;
            colPlayer.Update(player);
            colPlayerCurrency.Update(softCurrency);
            foreach (var deleteItemId in deleteItemIds)
            {
                colPlayerItem.Delete(deleteItemId);
            }
            var resultSoftCurrency = new PlayerCurrency();
            PlayerCurrency.CloneTo(softCurrency, resultSoftCurrency);
            result.updateCurrencies.Add(resultSoftCurrency);
            result.updateItems = updateItems;
            result.deleteItemIds = deleteItemIds;
        }
        onFinish(result);
    }

    protected override void DoEquipItem(string playerId, string loginToken, string characterId, string equipmentId, string equipPosition, UnityAction<ItemResult> onFinish)
    {
        var result = new ItemResult();
        var foundPlayer = colPlayer.FindOne(a => a.Id == playerId && a.LoginToken == loginToken);
        var foundCharacter = colPlayerItem.FindOne(a => a.Id == characterId && a.PlayerId == playerId);
        var foundEquipment = colPlayerItem.FindOne(a => a.Id == equipmentId && a.PlayerId == playerId);
        CharacterItem characterData = null;
        EquipmentItem equipmentData = null;
        if (foundCharacter != null)
        {
            var character = new PlayerItem();
            PlayerItem.CloneTo(foundCharacter, character);
            characterData = character.CharacterData;
        }
        if (foundEquipment != null)
        {
            var equipment = new PlayerItem();
            PlayerItem.CloneTo(foundEquipment, equipment);
            equipmentData = equipment.EquipmentData;
        }
        if (foundPlayer == null)
            result.error = GameServiceErrorCode.INVALID_LOGIN_TOKEN;
        else if (foundCharacter == null || foundEquipment == null)
            result.error = GameServiceErrorCode.INVALID_PLAYER_ITEM_DATA;
        else if (characterData == null || equipmentData == null)
            result.error = GameServiceErrorCode.INVALID_ITEM_DATA;
        else if (!equipmentData.equippablePositions.Contains(equipPosition))
            result.error = GameServiceErrorCode.INVALID_EQUIP_POSITION;
        else
        {
            result.updateItems = new List<PlayerItem>();
            var unEquipItem = colPlayerItem.FindOne(a => a.EquipItemId == characterId && a.EquipPosition == equipPosition && a.PlayerId == playerId);
            if (unEquipItem != null)
            {
                unEquipItem.EquipItemId = "";
                unEquipItem.EquipPosition = "";
                colPlayerItem.Update(unEquipItem);
                var resultUnEquipItem = new PlayerItem();
                PlayerItem.CloneTo(unEquipItem, resultUnEquipItem);
                result.updateItems.Add(resultUnEquipItem);
            }
            foundEquipment.EquipItemId = characterId;
            foundEquipment.EquipPosition = equipPosition;
            colPlayerItem.Update(foundEquipment);
            var resultEquipment = new PlayerItem();
            PlayerItem.CloneTo(foundEquipment, resultEquipment);
            result.updateItems.Add(resultEquipment);
        }
        onFinish(result);
    }

    protected override void DoUnEquipItem(string playerId, string loginToken, string equipmentId, UnityAction<ItemResult> onFinish)
    {
        var result = new ItemResult();
        var player = colPlayer.FindOne(a => a.Id == playerId && a.LoginToken == loginToken);
        var unEquipItem = colPlayerItem.FindOne(a => a.Id == equipmentId && a.PlayerId == playerId);
        if (player == null)
            result.error = GameServiceErrorCode.INVALID_LOGIN_TOKEN;
        else if (unEquipItem == null)
            result.error = GameServiceErrorCode.INVALID_PLAYER_ITEM_DATA;
        else
        {
            result.updateItems = new List<PlayerItem>();
            unEquipItem.EquipItemId = "";
            unEquipItem.EquipPosition = "";
            colPlayerItem.Update(unEquipItem);
            var resultItem = new PlayerItem();
            PlayerItem.CloneTo(unEquipItem, resultItem);
            result.updateItems.Add(resultItem);
        }
        onFinish(result);
    }

    protected override void DoGetAvailableLootBoxList(UnityAction<AvailableLootBoxListResult> onFinish)
    {
        var result = new AvailableLootBoxListResult();
        var gameDb = GameInstance.GameDatabase;
        result.list.AddRange(gameDb.LootBoxes.Keys);
        onFinish(result);
    }

    protected override void DoOpenLootBox(string playerId, string loginToken, string lootBoxDataId, UnityAction<ItemResult> onFinish)
    {
        var result = new ItemResult();
        var gameDb = GameInstance.GameDatabase;
        var player = colPlayer.FindOne(a => a.Id == playerId && a.LoginToken == loginToken);
        LootBox lootBox;
        if (player == null)
            result.error = GameServiceErrorCode.INVALID_LOGIN_TOKEN;
        else if (!gameDb.LootBoxes.TryGetValue(lootBoxDataId, out lootBox))
            result.error = GameServiceErrorCode.INVALID_LOOT_BOX_DATA;
        else
        {
            var softCurrency = GetCurrency(playerId, gameDb.softCurrency.id);
            var hardCurrency = GetCurrency(playerId, gameDb.hardCurrency.id);
            var requirementType = lootBox.requirementType;
            var requireCurrencyAmount = lootBox.requireCurrencyAmount;
            if (requirementType == LootBoxRequirementType.RequireSoftCurrency && requireCurrencyAmount > softCurrency.Amount)
                result.error = GameServiceErrorCode.NOT_ENOUGH_SOFT_CURRENCY;
            else if (requirementType == LootBoxRequirementType.RequireHardCurrency && requireCurrencyAmount > hardCurrency.Amount)
                result.error = GameServiceErrorCode.NOT_ENOUGH_HARD_CURRENCY;
            else
            {
                var resultCurrency = new PlayerCurrency();
                switch (requirementType)
                {
                    case LootBoxRequirementType.RequireSoftCurrency:
                        softCurrency.Amount -= requireCurrencyAmount;
                        colPlayerCurrency.Update(softCurrency);
                        PlayerCurrency.CloneTo(softCurrency, resultCurrency);
                        result.updateCurrencies.Add(resultCurrency);
                        break;
                    case LootBoxRequirementType.RequireHardCurrency:
                        hardCurrency.Amount -= requireCurrencyAmount;
                        colPlayerCurrency.Update(hardCurrency);
                        PlayerCurrency.CloneTo(hardCurrency, resultCurrency);
                        result.updateCurrencies.Add(resultCurrency);
                        break;
                }

                var rewardItem = lootBox.RandomReward().rewardItem;

                var createItems = new List<DbPlayerItem>();
                var updateItems = new List<DbPlayerItem>();
                if (AddItems(playerId, rewardItem.item.Id, rewardItem.amount, out createItems, out updateItems))
                {

                    foreach (var createEntry in createItems)
                    {
                        createEntry.Id = System.Guid.NewGuid().ToString();
                        colPlayerItem.Insert(createEntry);
                        var resultItem = new PlayerItem();
                        PlayerItem.CloneTo(createEntry, resultItem);
                        result.createItems.Add(resultItem);
                        HelperUnlockItem(player.Id, rewardItem.item.Id);
                    }
                    foreach (var updateEntry in updateItems)
                    {
                        colPlayerItem.Update(updateEntry);
                        var resultItem = new PlayerItem();
                        PlayerItem.CloneTo(updateEntry, resultItem);
                        result.updateItems.Add(resultItem);
                    }
                }
            }
        }
        onFinish(result);
    }
}
