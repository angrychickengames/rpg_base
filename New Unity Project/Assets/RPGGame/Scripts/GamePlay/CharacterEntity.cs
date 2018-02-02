using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(TargetingRigidbody))]
public class CharacterEntity : MonoBehaviour
{
    public const string ANIM_KEY_IS_DEAD = "IsDead";
    public const string ANIM_KEY_SPEED = "Speed";
    public const string ANIM_KEY_ACTION_STATE = "ActionState";
    public const string ANIM_KEY_HURT = "Hurt";
    public const int ACTION_ATTACK = -100;
    [Tooltip("The transform where we're going to spawn uis")]
    public Transform uiContainer;
    [Tooltip("The transform where we're going to spawn body effects")]
    public Transform bodyEffectContainer;
    [Tooltip("The transform where we're going to spawn floor effects")]
    public Transform floorEffectContainer;
    [Tooltip("The transform where we're going to spawn damage")]
    public Transform damageContainer;

    [HideInInspector]
    public bool forcePlayMoving;
    [HideInInspector]
    public bool forceHideCharacterStats;
    [HideInInspector]
    public int currentTimeCount;
    [HideInInspector]
    public bool selectable;
    [HideInInspector]
    public UICharacterStats uiCharacterStats;

    public GamePlayFormation Formation { get; private set; }
    public GamePlayManager Manager { get { return GamePlayManager.Singleton; } }
    public bool IsActiveCharacter { get { return Manager.ActiveCharacter == this; } }
    public bool IsPlayerCharacter { get { return Formation != null && Formation.isPlayerFormation; } }
    public int Position { get; private set; }
    public int Action { get; private set; }
    public bool IsDoingAction { get; private set; }
    public bool IsMovingToTarget { get; private set; }
    public CharacterSkill SelectedSkill { get { return Skills[Action]; } }
    public CharacterEntity ActionTarget { get; private set; }
    public readonly List<Damage> Damages = new List<Damage>();
    public readonly Dictionary<string, CharacterBuff> Buffs = new Dictionary<string, CharacterBuff>();
    public readonly List<CharacterSkill> Skills = new List<CharacterSkill>();
    public AttackAnimationData AttackAnimation { get { return Item.CharacterData.AttackAnimation; } }
    private PlayerItem item;
    public PlayerItem Item
    {
        get { return item; }
        set
        {
            if (value == null || item == value || value.CharacterData == null)
                return;
            item = value;
            Skills.Clear();
            var skills = item.CharacterData.skills;
            foreach (var skill in skills)
            {
                if (skill != null)
                    Skills.Add(new CharacterSkill(skill));
            }
            Revive();
        }
    }
    private Transform container;
    public Transform Container
    {
        get { return container; }
        set
        {
            container = value;
            TempTransform.SetParent(container);
            TempTransform.localPosition = Vector3.zero;
            TempTransform.localEulerAngles = Vector3.zero;
            TempTransform.localScale = Vector3.one;
            gameObject.SetActive(true);
        }
    }
    public int MaxHp
    {
        get { return GetTotalAttributes().hp; }
    }
    private int hp;
    public int Hp
    {
        get { return hp; }
        set
        {
            hp = value;
            if (hp <= 0)
                Dead();
            if (hp >= MaxHp)
                hp = MaxHp;
        }
    }

    private Transform tempTransform;
    public Transform TempTransform
    {
        get
        {
            if (tempTransform == null)
                tempTransform = GetComponent<Transform>();
            return tempTransform;
        }
    }
    private Animator tempAnimator;
    public Animator TempAnimator
    {
        get
        {
            if (tempAnimator == null)
                tempAnimator = GetComponent<Animator>();
            return tempAnimator;
        }
    }
    private Rigidbody tempRigidbody;
    public Rigidbody TempRigidbody
    {
        get
        {
            if (tempRigidbody == null)
                tempRigidbody = GetComponent<Rigidbody>();
            return tempRigidbody;
        }
    }
    private CapsuleCollider tempCapsuleCollider;
    public CapsuleCollider TempCapsuleCollider
    {
        get
        {
            if (tempCapsuleCollider == null)
                tempCapsuleCollider = GetComponent<CapsuleCollider>();
            return tempCapsuleCollider;
        }
    }
    private TargetingRigidbody tempTargetingRigidbody;
    public TargetingRigidbody TempTargetingRigidbody
    {
        get
        {
            if (tempTargetingRigidbody == null)
                tempTargetingRigidbody = GetComponent<TargetingRigidbody>();
            return tempTargetingRigidbody;
        }
    }
    private Vector3 targetPosition;
    private CharacterEntity targetCharacter;
    private Coroutine movingCoroutine;
    private bool isReachedTargetCharacter;

    #region Unity Functions
    private void Awake()
    {
        if (uiContainer == null)
            uiContainer = TempTransform;
        if (bodyEffectContainer == null)
            bodyEffectContainer = TempTransform;
        if (floorEffectContainer == null)
            floorEffectContainer = TempTransform;
        if (damageContainer == null)
            damageContainer = TempTransform;
        TempCapsuleCollider.isTrigger = true;
    }

    private void Update()
    {
        if (Item == null)
        {
            // For show in viewers
            TempAnimator.SetBool(ANIM_KEY_IS_DEAD, false);
            TempAnimator.SetFloat(ANIM_KEY_SPEED, 0);
            return;
        }
        TempAnimator.SetBool(ANIM_KEY_IS_DEAD, Hp <= 0);
        if (Hp > 0)
        {
            var moveSpeed = TempRigidbody.velocity.magnitude;
            // Assume that character is moving by set moveSpeed = 1
            if (forcePlayMoving)
                moveSpeed = 1;
            TempAnimator.SetFloat(ANIM_KEY_SPEED, moveSpeed);
            if (uiCharacterStats != null)
            {
                if (forceHideCharacterStats)
                    uiCharacterStats.Hide();
                else
                    uiCharacterStats.Show();
            }
        }
        else
        {
            if (uiCharacterStats != null)
                uiCharacterStats.Hide();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (targetCharacter != null && targetCharacter == other.GetComponent<CharacterEntity>())
            isReachedTargetCharacter = true;
    }

    private void OnDestroy()
    {
        if (uiCharacterStats != null)
            Destroy(uiCharacterStats.gameObject);
    }
    #endregion

    #region Damage/Dead/Revive/Turn/Buff
    public void Attack(CharacterEntity target, float pAtkRate = 1f, float mAtkRate = 1f, int hitCount = 1, int fixDamage = 0)
    {
        if (target == null)
            return;
        var attributes = GetTotalAttributes();
        target.ReceiveDamage(
            Mathf.CeilToInt(attributes.pAtk * pAtkRate),
            Mathf.CeilToInt(attributes.mAtk * mAtkRate),
            attributes.acc,
            attributes.critChance,
            attributes.critDamageRate,
            hitCount,
            fixDamage);
    }

    public void Attack(CharacterEntity target, Damage damagePrefab, float pAtkRate = 1f, float mAtkRate = 1f, int hitCount = 1, int fixDamage = 0)
    {
        if (damagePrefab == null)
            Attack(target, pAtkRate, mAtkRate, hitCount, fixDamage);
        else
        {
            var damage = Instantiate(damagePrefab, damageContainer.position, damageContainer.rotation);
            damage.Setup(this, target);
        }
    }

    public void ReceiveDamage(int pAtk, int mAtk, int acc, float critChance, float critDamageRate, int hitCount = 1, int fixDamage = 0)
    {
        if (hitCount <= 0)
            hitCount = 1;
        var gameDb = GameInstance.GameDatabase;
        var attributes = GetTotalAttributes();
        var pDmg = pAtk - attributes.pDef;
        var mDmg = mAtk - attributes.mDef;
        if (pDmg < 0)
            pDmg = 0;
        if (mDmg < 0)
            mDmg = 0;
        var totalDmg = pDmg + mDmg;
        var isCritical = false;
        var isBlock = false;
        totalDmg += Mathf.CeilToInt(totalDmg * Random.Range(gameDb.minAtkVaryRate, gameDb.maxAtkVaryRate)) + fixDamage;
        // Critical occurs
        if (Random.value <= critChance)
        {
            totalDmg = Mathf.CeilToInt(totalDmg * critDamageRate);
            isCritical = true;
        }
        // Block occurs
        if (Random.value <= attributes.blockChance)
        {
            totalDmg = Mathf.CeilToInt(totalDmg / attributes.blockDamageRate);
            isBlock = true;
        }
        // Accuracy should not <= 0, if it's <= 0 assume that it can hit 100% so evadeChance = 0
        var evadeChance = 0;
        if (acc > 0)
            evadeChance = (acc - attributes.eva) / acc;
        // Cannot evade, receive damage
        if (Random.value > evadeChance)
        {
            if (isBlock)
                Manager.SpawnBlockText(totalDmg, this);
            else if (isCritical)
                Manager.SpawnCriticalText(totalDmg, this);
            else
                Manager.SpawnDamageText(totalDmg, this);

            Hp -= totalDmg;
        }
        // Play hurt animation
        TempAnimator.ResetTrigger(ANIM_KEY_HURT);
        TempAnimator.SetTrigger(ANIM_KEY_HURT);
    }

    public void Dead()
    {
        var keys = new List<string>(Buffs.Keys);
        for (var i = keys.Count - 1; i >= 0; --i)
        {
            var key = keys[i];
            if (!Buffs.ContainsKey(key))
                continue;

            var buff = Buffs[key];
            buff.BuffRemove();
            Buffs.Remove(key);
        }
        ClearActionState();
    }

    public void Revive()
    {
        if (Item == null)
            return;

        Hp = MaxHp;
    }

    public void DecreaseBuffsTurn()
    {
        var keys = new List<string>(Buffs.Keys);
        for (var i = keys.Count - 1; i >= 0; --i)
        {
            var key = keys[i];
            if (!Buffs.ContainsKey(key))
                continue;

            var buff = Buffs[key];
            buff.IncreaseTurnsCount();
            if (buff.IsEnd())
            {
                buff.BuffRemove();
                Buffs.Remove(key);
            }
        }
    }

    public void DecreaseSkillsTurn()
    {
        for (var i = Skills.Count - 1; i >= 0; --i)
        {
            var skill = Skills[i];
            skill.IncreaseTurnsCount();
        }
    }

    public void ApplyBuff(CharacterEntity caster, Skill skill, int buffIndex)
    {
        if (skill == null || buffIndex < 0 || buffIndex >= skill.buffs.Length || skill.buffs[buffIndex] == null || Hp <= 0)
            return;

        var buff = new CharacterBuff(caster, this, skill, buffIndex);
        if (buff.Buff.applyTurns > 0)
        {
            // Buff cannot stack so remove old buff
            if (Buffs.ContainsKey(buff.Id))
            {
                buff.BuffRemove();
                Buffs.Remove(buff.Id);
            }
            Buffs[buff.Id] = buff;
        }
    }
    #endregion

    #region Movement/Actions
    public Coroutine MoveTo(Vector3 position, float speed)
    {
        if (IsMovingToTarget)
            StopCoroutine(movingCoroutine);
        IsMovingToTarget = true;
        isReachedTargetCharacter = false;
        targetPosition = position;
        movingCoroutine = StartCoroutine(MoveToRoutine(position, speed));
        return movingCoroutine;
    }

    IEnumerator MoveToRoutine(Vector3 position, float speed)
    {
        TempTargetingRigidbody.StartMove(position, speed);
        while (true)
        {
            if (!TempTargetingRigidbody.IsMoving || isReachedTargetCharacter)
            {
                IsMovingToTarget = false;
                TempTargetingRigidbody.StopMove();
                if (targetCharacter == null)
                {
                    TurnToEnemyFormation();
                    TempTransform.position = targetPosition;
                }
                targetCharacter = null;
                break;
            }
            yield return 0;
        }
    }

    public Coroutine MoveTo(CharacterEntity character, float speed)
    {
        targetCharacter = character;
        return MoveTo(character.TempTransform.position, speed);
    }

    public void TurnToEnemyFormation()
    {
        Quaternion headingRotation;
        if (Formation.TryGetHeadingToFoeRotation(out headingRotation))
            TempTransform.rotation = headingRotation;
    }

    public void ClearActionState()
    {
        TempAnimator.SetInteger(ANIM_KEY_ACTION_STATE, 0);
    }

    public bool SetAction(int action)
    {
        if (action == ACTION_ATTACK || (action >= 0 && action < Skills.Count))
        {
            Action = action;
            Manager.ShowTargetScopesOrDoAction(this);
            return true;
        }
        return false;
    }

    public bool DoAction(CharacterEntity target)
    {
        if (target == null || target.Hp <= 0)
            return false;

        if (Action == ACTION_ATTACK)
        {
            // Cannot attack self or same team character
            if (target == this || IsSameTeamWith(target))
                return false;
        }
        else
        {
            if (SelectedSkill == null || !SelectedSkill.IsReady())
                return false;

            switch (SelectedSkill.Skill.usageScope)
            {
                case SkillUsageScope.Self:
                    if (target != this)
                        return false;
                    break;
                case SkillUsageScope.Enemy:
                    if (target == this || IsSameTeamWith(target))
                        return false;
                    break;
                case SkillUsageScope.Ally:
                    if (!IsSameTeamWith(target))
                        return false;
                    break;
            }
        }
        ActionTarget = target;
        DoAction();
        return true;
    }

    public void RandomAction()
    {
        // Random Action
        // Dictionary of actionId, weight
        Dictionary<int, int> actions = new Dictionary<int, int>();
        actions.Add(ACTION_ATTACK, 5);
        for (var i = 0; i < Skills.Count; ++i)
        {
            var skill = Skills[i];
            if (skill == null || !skill.IsReady())
                continue;
            actions.Add(i, 5);
        }
        Action = WeightedRandomizer.From(actions).TakeOne();
        // Random Target
        if (Action == ACTION_ATTACK)
        {
            var foes = Manager.GetFoes(this);
            Random.InitState(System.DateTime.Now.Millisecond);
            ActionTarget = foes[Random.Range(0, foes.Count - 1)];
        }
        else
        {
            switch (SelectedSkill.Skill.usageScope)
            {
                case SkillUsageScope.Enemy:
                    var foes = Manager.GetFoes(this);
                    Random.InitState(System.DateTime.Now.Millisecond);
                    ActionTarget = foes[Random.Range(0, foes.Count)];
                    break;
                case SkillUsageScope.Ally:
                    var allies = Manager.GetAllies(this);
                    Random.InitState(System.DateTime.Now.Millisecond);
                    ActionTarget = allies[Random.Range(0, allies.Count)];
                    break;
                default:
                    ActionTarget = null;
                    break;
            }
        }
        DoAction();
    }

    private void DoAction()
    {
        if (IsDoingAction)
            return;

        if (Action == ACTION_ATTACK)
            StartCoroutine(DoAttackActionRoutine());
        else
        {
            SelectedSkill.OnUseSkill();
            StartCoroutine(DoSkillActionRoutine());
        }
    }

    IEnumerator DoAttackActionRoutine()
    {
        IsDoingAction = true;
        var manager = Manager;
        if (!AttackAnimation.isRangeAttack)
        {
            // Move to target character
            yield return MoveTo(ActionTarget, Manager.doActionMoveSpeed);
        }
        // Play attack animation
        TempAnimator.SetInteger(ANIM_KEY_ACTION_STATE, AttackAnimation.animationActionState);
        yield return new WaitForSeconds(AttackAnimation.hitDuration);
        // Apply damage
        Attack(ActionTarget, AttackAnimation.damage);
        // Wait damages done
        while (Damages.Count > 0)
        {
            yield return 0;
        }
        // End attack
        var endAttackDuration = AttackAnimation.animationDuration - AttackAnimation.hitDuration;
        if (endAttackDuration < 0)
            endAttackDuration = 0;
        yield return new WaitForSeconds(endAttackDuration);
        ClearActionState();
        yield return MoveTo(Container.position, Manager.actionDoneMoveSpeed);
        NotifyEndAction();
        IsDoingAction = false;
    }

    IEnumerator DoSkillActionRoutine()
    {
        IsDoingAction = true;
        var skill = SelectedSkill.Skill;
        var manager = Manager;
        // Cast
        if (skill.CastAnimation.castAtMapCenter)
            yield return MoveTo(Manager.MapCenterPosition, Manager.doActionMoveSpeed);
        TempAnimator.SetInteger(ANIM_KEY_ACTION_STATE, skill.CastAnimation.animationActionState);
        yield return new WaitForSeconds(skill.CastAnimation.animationDuration);
        // Attacks
        yield return StartCoroutine(SkillAttackRoutine());
        // Buffs
        yield return StartCoroutine(ApplyBuffsRoutine());
        // Move back to formation
        yield return MoveTo(Container.position, Manager.actionDoneMoveSpeed);
        NotifyEndAction();
        IsDoingAction = false;
    }

    IEnumerator ApplyBuffsRoutine()
    {
        var skill = SelectedSkill.Skill;
        for (var i = 0; i < skill.buffs.Length; ++i)
        {
            var buff = skill.buffs[i];
            if (buff == null)
                continue;

            var allies = Manager.GetAllies(this);
            var foes = Manager.GetFoes(this);
            if (Random.value <= buff.applyChance)
            {
                // Apply buffs to selected targets
                switch (buff.buffScope)
                {
                    case BuffScope.SelectedTarget:
                    case BuffScope.SelectedAndOneRandomTargets:
                    case BuffScope.SelectedAndTwoRandomTargets:
                    case BuffScope.SelectedAndThreeRandomTargets:
                        ActionTarget.ApplyBuff(this, skill, i);
                        break;
                }

                int randomAllyCount = 0;
                int randomFoeCount = 0;
                // Buff scope
                switch (buff.buffScope)
                {
                    case BuffScope.Self:
                        ApplyBuff(this, skill, i);
                        continue;
                    case BuffScope.SelectedAndOneRandomTargets:
                        if (ActionTarget.IsSameTeamWith(this))
                            randomAllyCount = 1;
                        else if (!ActionTarget.IsSameTeamWith(this))
                            randomFoeCount = 1;
                        break;
                    case BuffScope.SelectedAndTwoRandomTargets:
                        if (ActionTarget.IsSameTeamWith(this))
                            randomAllyCount = 2;
                        else if (!ActionTarget.IsSameTeamWith(this))
                            randomFoeCount = 2;
                        break;
                    case BuffScope.SelectedAndThreeRandomTargets:
                        if (ActionTarget.IsSameTeamWith(this))
                            randomAllyCount = 3;
                        else if (!ActionTarget.IsSameTeamWith(this))
                            randomFoeCount = 3;
                        break;
                    case BuffScope.OneRandomAlly:
                        randomAllyCount = 1;
                        break;
                    case BuffScope.TwoRandomAllies:
                        randomAllyCount = 2;
                        break;
                    case BuffScope.ThreeRandomAllies:
                        randomAllyCount = 3;
                        break;
                    case BuffScope.FourRandomAllies:
                        randomAllyCount = 4;
                        break;
                    case BuffScope.AllAllies:
                        randomAllyCount = allies.Count;
                        break;
                    case BuffScope.OneRandomEnemy:
                        randomFoeCount = 1;
                        break;
                    case BuffScope.TwoRandomEnemies:
                        randomFoeCount = 2;
                        break;
                    case BuffScope.ThreeRandomEnemies:
                        randomFoeCount = 3;
                        break;
                    case BuffScope.FourRandomEnemies:
                        randomFoeCount = 4;
                        break;
                    case BuffScope.AllEnemies:
                        randomFoeCount = foes.Count;
                        break;
                    case BuffScope.All:
                        randomAllyCount = allies.Count;
                        randomFoeCount = foes.Count;
                        break;
                }
                // End buff scope
                // Don't apply buffs to character that already applied
                if (randomAllyCount > 0)
                {
                    allies.Remove(ActionTarget);
                    while (allies.Count > 0 && randomAllyCount > 0)
                    {
                        Random.InitState(System.DateTime.Now.Millisecond);
                        var randomIndex = Random.Range(0, allies.Count - 1);
                        var applyBuffTarget = allies[randomIndex];
                        applyBuffTarget.ApplyBuff(this, skill, i);
                        allies.RemoveAt(randomIndex);
                        --randomAllyCount;
                    }
                }
                // Don't apply buffs to character that already applied
                if (randomFoeCount > 0)
                {
                    foes.Remove(ActionTarget);
                    while (foes.Count > 0 && randomFoeCount > 0)
                    {
                        Random.InitState(System.DateTime.Now.Millisecond);
                        var randomIndex = Random.Range(0, foes.Count - 1);
                        var applyBuffTarget = foes[randomIndex];
                        applyBuffTarget.ApplyBuff(this, skill, i);
                        foes.RemoveAt(randomIndex);
                        --randomFoeCount;
                    }
                }
            }
        }
        yield return 0;
    }

    IEnumerator SkillAttackRoutine()
    {
        var skill = SelectedSkill.Skill;
        if (skill.attacks.Length > 0)
        {
            var foes = Manager.GetFoes(this);
            var isAlreadyReachedTarget = false;
            foreach (var attack in skill.attacks)
            {
                if (attack == null)
                    continue;
                var attackDamage = attack.attackDamage;
                var attackAnimation = attack.AttackAnimation;
                if (!attackAnimation.isRangeAttack && !isAlreadyReachedTarget)
                {
                    // Move to target character
                    yield return MoveTo(ActionTarget, Manager.doActionMoveSpeed);
                    isAlreadyReachedTarget = true;
                }
                // Play attack animation
                TempAnimator.SetInteger(ANIM_KEY_ACTION_STATE, attackAnimation.animationActionState);
                yield return new WaitForSeconds(attackAnimation.hitDuration);
                // Apply damage
                // Attack to selected target
                switch (attack.attackScope)
                {
                    case AttackScope.SelectedTarget:
                    case AttackScope.SelectedAndOneRandomTargets:
                    case AttackScope.SelectedAndTwoRandomTargets:
                    case AttackScope.SelectedAndThreeRandomTargets:
                        Attack(ActionTarget, attackAnimation.damage, attackDamage.pAtkDamageRate, attackDamage.mAtkDamageRate, attackDamage.hitCount, attackDamage.fixDamage);
                        break;
                }
                // Attack to random targets
                int randomFoeCount = 0;
                // Attack scope
                switch (attack.attackScope)
                {
                    case AttackScope.SelectedAndOneRandomTargets:
                    case AttackScope.OneRandomEnemy:
                        randomFoeCount = 1;
                        break;
                    case AttackScope.SelectedAndTwoRandomTargets:
                    case AttackScope.TwoRandomEnemies:
                        randomFoeCount = 2;
                        break;
                    case AttackScope.SelectedAndThreeRandomTargets:
                    case AttackScope.ThreeRandomEnemies:
                        randomFoeCount = 3;
                        break;
                    case AttackScope.FourRandomEnemies:
                        randomFoeCount = 4;
                        break;
                    case AttackScope.AllEnemies:
                        randomFoeCount = foes.Count;
                        break;
                }
                // End attack scope
                while (foes.Count > 0 && randomFoeCount > 0)
                {
                    Random.InitState(System.DateTime.Now.Millisecond);
                    var randomIndex = Random.Range(0, foes.Count - 1);
                    var attackingTarget = foes[randomIndex];
                    Attack(attackingTarget, attackAnimation.damage, attackDamage.pAtkDamageRate, attackDamage.mAtkDamageRate, attackDamage.hitCount, attackDamage.fixDamage);
                    foes.RemoveAt(randomIndex);
                    --randomFoeCount;
                }
                // End attack
                var endAttackDuration = attackAnimation.animationDuration - attackAnimation.hitDuration;
                if (endAttackDuration < 0)
                    endAttackDuration = 0;
                yield return new WaitForSeconds(endAttackDuration);
                ClearActionState();
                yield return 0;
            }
            // End attack loop
            // Wait damages done
            while (Damages.Count > 0)
            {
                yield return 0;
            }
        }
    }

    public void ResetStates()
    {
        Action = ACTION_ATTACK;
        ActionTarget = null;
        IsDoingAction = false;
    }

    public void NotifyEndAction()
    {
        Manager.NotifyEndAction(this);
    }
    #endregion

    #region Misc
    public bool IsSameTeamWith(CharacterEntity target)
    {
        return target != null && Formation == target.Formation;
    }

    public void SetFormation(GamePlayFormation formation, int position)
    {
        if (formation == null || position < 0 || position >= formation.containers.Length)
            return;

        Formation = formation;
        Position = position;
        Container = formation.containers[position];

        Quaternion headingRotation;
        if (Formation.TryGetHeadingToFoeRotation(out headingRotation))
        {
            TempTransform.rotation = headingRotation;
            if (Manager != null)
                TempTransform.position -= Manager.spawnOffset * TempTransform.forward;
        }
    }

    public CalculationAttributes GetTotalAttributes()
    {
        var result = Item.Attributes;
        var equipmentBonus = Item.EquipmentBonus;
        result += equipmentBonus;

        var buffs = Buffs.Values.ToList();
        foreach (var buff in buffs)
        {
            result += buff.Buff.attributes;
        }

        // If this is character item, applies rate attributes
        result.hp += Mathf.CeilToInt(result.hpRate * result.hp);
        result.pAtk += Mathf.CeilToInt(result.pAtkRate * result.pAtk);
        result.pDef += Mathf.CeilToInt(result.pDefRate * result.pDef);
        result.mAtk += Mathf.CeilToInt(result.mAtkRate * result.mAtk);
        result.mDef += Mathf.CeilToInt(result.mDefRate * result.mDef);
        result.spd += Mathf.CeilToInt(result.spdRate * result.spd);
        result.eva += Mathf.CeilToInt(result.evaRate * result.eva);
        result.acc += Mathf.CeilToInt(result.accRate * result.acc);
        result.hpRate = 0;
        result.pAtkRate = 0;
        result.pDefRate = 0;
        result.mAtkRate = 0;
        result.mDefRate = 0;
        result.spdRate = 0;
        result.evaRate = 0;
        result.accRate = 0;

        return result;
    }
    #endregion
}
