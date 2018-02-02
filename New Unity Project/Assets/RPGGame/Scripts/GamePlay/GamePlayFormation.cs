using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GamePlayFormation : MonoBehaviour
{
    public Transform[] containers;
    public GamePlayFormation foeFormation;
    public bool isPlayerFormation;
    public GamePlayManager Manager { get { return GamePlayManager.Singleton; } }
    public readonly Dictionary<int, CharacterEntity> Characters = new Dictionary<int, CharacterEntity>();
    public readonly Dictionary<int, UICharacterStats> UIStats = new Dictionary<int, UICharacterStats>();

    private void Start()
    {
        if (isPlayerFormation)
            SetFormationCharacters();
    }

    public void SetFormationCharacters()
    {
        var formationName = Player.CurrentPlayer.SelectedFormation;
        ClearCharacters();
        for (var i = 0; i < containers.Length; ++i)
        {
            PlayerFormation playerFormation = null;
            if (PlayerFormation.TryGetData(formationName, i, out playerFormation))
            {
                var itemId = playerFormation.ItemId;
                PlayerItem item = null;
                if (!string.IsNullOrEmpty(itemId) && PlayerItem.DataMap.TryGetValue(itemId, out item))
                    SetCharacter(i, item);
            }
        }
    }

    public void SetCharacters(PlayerItem[] items)
    {
        ClearCharacters();
        for (var i = 0; i < containers.Length; ++i)
        {
            if (items.Length <= i)
                break;
            var item = items[i];
            SetCharacter(i, item);
        }
    }

    public void SetCharacter(int position, PlayerItem item)
    {
        if (position < 0 || position >= containers.Length || item == null || item.CharacterData == null)
            return;

        if (item.CharacterData.model == null)
        {
            Debug.LogWarning("Character's model is empty, this MUST be set");
            return;
        }

        var container = containers[position];
        container.RemoveAllChildren();

        UICharacterStats uiStats;
        if (UIStats.TryGetValue(position, out uiStats))
        {
            Destroy(uiStats.gameObject);
            UIStats.Remove(position);
        }

        var character = Instantiate(item.CharacterData.model);
        character.SetFormation(this, position);
        character.Item = item;
        Characters[position] = character;

        if (Manager != null)
        {
            uiStats = Instantiate(Manager.uiCharacterStatsPrefab, Manager.uiCharacterStatsContainer);
            uiStats.transform.localScale = Vector3.one;
            uiStats.character = character;
            character.uiCharacterStats = uiStats;
        }
    }

    public void ClearCharacters()
    {
        foreach (var container in containers)
        {
            container.RemoveAllChildren();
        }
        Characters.Clear();
    }

    public void Revive()
    {
        var characters = Characters.Values;
        foreach (var character in characters)
        {
            character.Revive();
        }
    }

    public bool IsAnyCharacterAlive()
    {
        var characters = Characters.Values;
        foreach (var character in characters)
        {
            if (character.Hp > 0)
                return true;
        }
        return false;
    }

    public bool TryGetHeadingToFoeRotation(out Quaternion rotation)
    {
        if (foeFormation != null)
        {
            var rotateHeading = foeFormation.transform.position - transform.position;
            rotation = Quaternion.LookRotation(rotateHeading);
            return true;
        }
        rotation = Quaternion.identity;
        return false;
    }

    public Coroutine MoveCharactersToFormation(bool stillForceMoving)
    {
        return StartCoroutine(MoveCharactersToFormationRoutine(stillForceMoving));
    }

    private IEnumerator MoveCharactersToFormationRoutine(bool stillForceMoving)
    {
        var characters = Characters.Values;
        foreach (var character in characters)
        {
            character.forcePlayMoving = stillForceMoving;
            character.MoveTo(character.Container.position, Manager.formationMoveSpeed);
        }
        while (true)
        {
            yield return 0;
            var ifEveryoneReachedTarget = true;
            foreach (var character in characters)
            {
                if (character.IsMovingToTarget)
                {
                    ifEveryoneReachedTarget = false;
                    break;
                }
            }
            if (ifEveryoneReachedTarget)
                break;
        }
    }

    public void SetActiveDeadCharacters(bool isActive)
    {
        var characters = Characters.Values;
        foreach (var character in characters)
        {
            if (character.Hp <= 0)
                character.gameObject.SetActive(isActive);
        }
    }

    public Coroutine ForceCharactersPlayMoving(float duration)
    {
        return StartCoroutine(ForceCharactersPlayMovingRoutine(duration));
    }

    private IEnumerator ForceCharactersPlayMovingRoutine(float duration)
    {
        var characters = Characters.Values;
        foreach (var character in characters)
        {
            character.forcePlayMoving = true;
        }
        yield return new WaitForSeconds(duration);
        foreach (var character in characters)
        {
            character.forcePlayMoving = false;
        }
    }

    public void SetCharactersSelectable(bool selectable)
    {
        var characters = Characters.Values;
        foreach (var character in characters)
        {
            character.selectable = selectable;
        }
    }

    public int CountDeadCharacters()
    {
        return Characters.Values.Where(a => a.Hp <= 0).ToList().Count;
    }
}
