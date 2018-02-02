using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class GamePlayManager : MonoBehaviour
{
    public static string BattleSession { get; private set; }
    public static Stage PlayingStage { get; private set; }
    public static GamePlayManager Singleton { get; private set; }
    public Camera inputCamera;
    [Header("Formation/Spawning")]
    public GamePlayFormation playerFormation;
    public GamePlayFormation foeFormation;
    public EnvironmentManager environmentManager;
    public Transform mapCenter;
    public float spawnOffset = 5f;
    [Header("Speed/Delay")]
    public float formationMoveSpeed = 5f;
    public float doActionMoveSpeed = 8f;
    public float actionDoneMoveSpeed = 10f;
    public float beforeMoveToNextWaveDelay = 2f;
    public float moveToNextWaveDelay = 1f;
    public float winGameDelay = 2f;
    public float loseGameDelay = 2f;
    [Header("UI")]
    public Transform uiCharacterStatsContainer;
    public UICharacterStats uiCharacterStatsPrefab;
    public Transform combatTextContainer;
    public UICombatText combatDamagePrefab;
    public UICombatText combatCriticalPrefab;
    public UICombatText combatBlockPrefab;
    public UICombatText combatHealPrefab;
    public UICombatText combatPoisonPrefab;
    public UICharacterActionManager uiCharacterActionManager;
    public UIWin uiWin;
    public UILose uiLose;
    public UIPauseGame uiPauseGame;
    public CharacterEntity ActiveCharacter { get; private set; }
    public int CurrentWave { get; private set; }
    public int MaxWave { get { return PlayingStage.waves.Length; } }
    public bool IsAutoPlay { get; set; }
    public bool IsSpeedMultiply { get; set; }
    private bool isAutoPlayDirty;
    private bool isEnding;
    public Vector3 MapCenterPosition
    {
        get
        {
            if (mapCenter == null)
                return Vector3.zero;
            return mapCenter.position;
        }
    }

    private void Awake()
    {
        Singleton = this;
        if (inputCamera == null)
            inputCamera = Camera.main;
        // Setup uis
        uiCharacterActionManager.Hide();
        // Setup player formation
        playerFormation.isPlayerFormation = true;
        playerFormation.foeFormation = foeFormation;
        // Setup foe formation
        foeFormation.ClearCharacters();
        foeFormation.isPlayerFormation = false;
        foeFormation.foeFormation = playerFormation;

        SetupEnvironment();
    }

    private void Start()
    {
        CurrentWave = 0;
        StartCoroutine(StartGame());
    }

    private void Update()
    {
        if (uiPauseGame.IsVisible())
        {
            Time.timeScale = 0;
            return;
        }

        if (IsAutoPlay != isAutoPlayDirty)
        {
            if (IsAutoPlay)
            {
                uiCharacterActionManager.Hide();
                if (ActiveCharacter != null)
                    ActiveCharacter.RandomAction();
            }
            isAutoPlayDirty = IsAutoPlay;
        }

        Time.timeScale = !isEnding && IsSpeedMultiply ? 2 : 1;

        if (Input.GetMouseButtonDown(0) && ActiveCharacter != null && ActiveCharacter.IsPlayerCharacter)
        {
            Ray ray = inputCamera.ScreenPointToRay(InputManager.MousePosition());
            RaycastHit hitInfo;
            if (!Physics.Raycast(ray, out hitInfo))
                return;

            var targetCharacter = hitInfo.collider.GetComponent<CharacterEntity>();
            if (targetCharacter != null)
            {
                if (ActiveCharacter.DoAction(targetCharacter))
                {
                    playerFormation.SetCharactersSelectable(false);
                    foeFormation.SetCharactersSelectable(false);
                }
            }
        }
    }

    IEnumerator StartGame()
    {
        yield return playerFormation.MoveCharactersToFormation(true);
        environmentManager.isPause = false;
        yield return playerFormation.ForceCharactersPlayMoving(moveToNextWaveDelay);
        environmentManager.isPause = true;
        NextWave();
        yield return foeFormation.MoveCharactersToFormation(false);
        NewTurn();
    }

    public void SetupEnvironment()
    {
        environmentManager.spawningObjects = PlayingStage.environment.environmentObjects;
        environmentManager.SpawnObjects();
        environmentManager.isPause = true;
    }

    public void NextWave()
    {
        PlayerItem[] characters;
        StageFoe[] foes;
        var wave = PlayingStage.waves[CurrentWave];
        if (!wave.useRandomFoes && wave.foes.Length > 0)
            foes = wave.foes;
        else
            foes = PlayingStage.RandomFoes().foes;

        characters = new PlayerItem[foes.Length];
        for (var i = 0; i < characters.Length; ++i)
        {
            var foe = foes[i];
            if (foe != null && foe.character != null)
            {
                var character = PlayerItem.CreateActorItemWithLevel(foe.character, foe.level);
                characters[i] = character;
            }
        }

        if (characters.Length == 0)
            Debug.LogError("Missing Foes Data");

        foeFormation.SetCharacters(characters);
        foeFormation.Revive();
        ++CurrentWave;
    }

    public void Revive(UnityAction onError)
    {
        GameInstance.GameService.ReviveCharacters((result) =>
        {
            isEnding = false;
            playerFormation.Revive();
            NewTurn();
        }, (error) =>
        {
            GameInstance.Singleton.OnGameServiceError(error, onError);
        });
    }

    public void Giveup(UnityAction onError)
    {
        var countDeadCharacter = playerFormation.CountDeadCharacters();
        GameInstance.GameService.FinishStage(BattleSession, BaseGameService.BATTLE_RESULT_LOSE, countDeadCharacter, (result) =>
        {
            isEnding = true;
            Time.timeScale = 1;
            GameInstance.Singleton.GetAllPlayerData(GameInstance.LoadAllPlayerDataState.GoToManageScene);
        }, (error) =>
        {
            GameInstance.Singleton.OnGameServiceError(error, onError);
        });
    }

    public void Restart()
    {
        StartStage(PlayingStage);
    }

    IEnumerator MoveToNextWave()
    {
        yield return new WaitForSeconds(beforeMoveToNextWaveDelay);
        foeFormation.ClearCharacters();
        playerFormation.SetActiveDeadCharacters(false);
        environmentManager.isPause = false;
        yield return playerFormation.ForceCharactersPlayMoving(moveToNextWaveDelay);
        environmentManager.isPause = true;
        playerFormation.SetActiveDeadCharacters(true);
        NextWave();
        yield return foeFormation.MoveCharactersToFormation(false);
        NewTurn();
    }

    IEnumerator WinGameRoutine()
    {
        isEnding = true;
        yield return new WaitForSeconds(winGameDelay);
        WinGame();
    }

    private void WinGame()
    {
        var countDeadCharacter = playerFormation.CountDeadCharacters();
        GameInstance.GameService.FinishStage(BattleSession, BaseGameService.BATTLE_RESULT_WIN, countDeadCharacter, (result) =>
        {
            isEnding = true;
            Time.timeScale = 1;
            GameInstance.Singleton.OnGameServiceFinishStageResult(result);
            uiWin.SetData(result);
            uiWin.Show();
        }, (error) =>
        {
            GameInstance.Singleton.OnGameServiceError(error, WinGame);
        });
    }

    IEnumerator LoseGameRoutine()
    {
        isEnding = true;
        yield return new WaitForSeconds(loseGameDelay);
        uiLose.Show();
    }

    public void NewTurn()
    {
        if (ActiveCharacter != null)
            ActiveCharacter.currentTimeCount = 0;

        CharacterEntity activatingCharacter = null;
        var maxTime = int.MinValue;
        List<CharacterEntity> characters = new List<CharacterEntity>();
        characters.AddRange(playerFormation.Characters.Values);
        characters.AddRange(foeFormation.Characters.Values);
        for (int i = 0; i < characters.Count; ++i)
        {
            CharacterEntity character = characters[i];
            if (character != null)
            {
                if (character.Hp > 0)
                {
                    int spd = character.GetTotalAttributes().spd;
                    if (spd <= 0)
                        spd = 1;
                    character.currentTimeCount += spd;
                    if (character.currentTimeCount > maxTime)
                    {
                        maxTime = character.currentTimeCount;
                        activatingCharacter = character;
                    }
                }
                else
                    character.currentTimeCount = 0;
            }
        }
        ActiveCharacter = activatingCharacter;
        ActiveCharacter.DecreaseBuffsTurn();
        ActiveCharacter.DecreaseSkillsTurn();
        ActiveCharacter.ResetStates();
        if (ActiveCharacter.Hp > 0)
        {
            if (ActiveCharacter.IsPlayerCharacter)
            {
                if (IsAutoPlay)
                    ActiveCharacter.RandomAction();
                else
                    uiCharacterActionManager.Show();
            }
            else
                ActiveCharacter.RandomAction();
        }
        else
            ActiveCharacter.NotifyEndAction();
    }

    /// <summary>
    /// This will be called by Character class to show target scopes or do action
    /// </summary>
    /// <param name="character"></param>
    public void ShowTargetScopesOrDoAction(CharacterEntity character)
    {
        var allyTeamFormation = character.IsPlayerCharacter ? playerFormation : foeFormation;
        var foeTeamFormation = !character.IsPlayerCharacter ? playerFormation : foeFormation;
        allyTeamFormation.SetCharactersSelectable(false);
        foeTeamFormation.SetCharactersSelectable(false);
        if (character.Action == CharacterEntity.ACTION_ATTACK)
            foeTeamFormation.SetCharactersSelectable(true);
        else
        {
            switch (character.SelectedSkill.Skill.usageScope)
            {
                case SkillUsageScope.Self:
                    character.selectable = true;
                    break;
                case SkillUsageScope.Ally:
                    allyTeamFormation.SetCharactersSelectable(true);
                    break;
                case SkillUsageScope.Enemy:
                    foeTeamFormation.SetCharactersSelectable(true);
                    break;
                case SkillUsageScope.All:
                    allyTeamFormation.SetCharactersSelectable(true);
                    foeTeamFormation.SetCharactersSelectable(true);
                    break;
            }
        }
    }

    public List<CharacterEntity> GetAllies(CharacterEntity character)
    {
        if (character.IsPlayerCharacter)
            return playerFormation.Characters.Values.Where(a => a.Hp > 0).ToList();
        else
            return foeFormation.Characters.Values.Where(a => a.Hp > 0).ToList();
    }

    public List<CharacterEntity> GetFoes(CharacterEntity character)
    {
        if (character.IsPlayerCharacter)
            return foeFormation.Characters.Values.Where(a => a.Hp > 0).ToList();
        else
            return playerFormation.Characters.Values.Where(a => a.Hp > 0).ToList();
    }

    public void NotifyEndAction(CharacterEntity character)
    {
        if (character != ActiveCharacter)
            return;

        if (!playerFormation.IsAnyCharacterAlive())
        {
            ActiveCharacter = null;
            StartCoroutine(LoseGameRoutine());
        }
        else if (!foeFormation.IsAnyCharacterAlive())
        {
            ActiveCharacter = null;
            if (CurrentWave >= PlayingStage.waves.Length)
            {
                StartCoroutine(WinGameRoutine());
                return;
            }
            StartCoroutine(MoveToNextWave());
        }
        else
            NewTurn();
    }

    public void SpawnDamageText(int amount, CharacterEntity character)
    {
        SpawnCombatText(combatDamagePrefab, amount, character);
    }

    public void SpawnCriticalText(int amount, CharacterEntity character)
    {
        SpawnCombatText(combatCriticalPrefab, amount, character);
    }

    public void SpawnBlockText(int amount, CharacterEntity character)
    {
        SpawnCombatText(combatBlockPrefab, amount, character);
    }

    public void SpawnHealText(int amount, CharacterEntity character)
    {
        SpawnCombatText(combatHealPrefab, amount, character);
    }

    public void SpawnPoisonText(int amount, CharacterEntity character)
    {
        SpawnCombatText(combatPoisonPrefab, amount, character);
    }

    public void SpawnCombatText(UICombatText prefab, int amount, CharacterEntity character)
    {
        var combatText = Instantiate(prefab, combatTextContainer);
        combatText.transform.localScale = Vector3.one;
        combatText.TempObjectFollower.targetObject = character.bodyEffectContainer;
        combatText.Amount = amount;
    }

    public static void StartStage(Stage data)
    {
        PlayingStage = data;
        GameInstance.GameService.StartStage(data.Id, (result) =>
        {
            GameInstance.Singleton.OnGameServiceStartStageResult(result);
            BattleSession = result.session;
            GameInstance.Singleton.LoadBattleScene();
        }, (error) =>
        {
            GameInstance.Singleton.OnGameServiceError(error);
        });
    }
}
