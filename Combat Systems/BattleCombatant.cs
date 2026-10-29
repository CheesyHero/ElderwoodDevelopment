using System;
using System.Collections;
using System.Collections.Generic; 
using UnityEngine; 

[SelectionBase]
public class BattleCombatant : MonoBehaviour
{
    public BattleUnit parent;

    private readonly static List<BattleCombatant> all = new();
    public static BattleCombatant[] AllCombatants 
    { get { return all.ToArray(); } }

    public bool idled;
    public SpriteRenderer artRenderer;
    public AnimationHandler animations;
    public int actionPoints;
    public int reactionPoints;

    [Header("Main")]
    public Hero hero = new(1);
    public Attributes modifiers = new(1);
    public Team team = new();
    public bool isMinion = false;
    [Header("Art")]
    public float walkTime = 0.5f;
    public float settleTime = 0.5f;
    public float waitTime = 0.5f;
    public float travelOffset = 1.5f;

    public ParticleSystem actionEffect; // Plays when a move starts
    public ParticleSystem reactionEffect; // Plays when a reaction starts
    public ParticleSystem specialEffect; // Plays when a specail move starts
    public ParticleSystem chargingEffect; // Plays when charging a move

    public Transform particlePoint;

    [Header("Statistics")]
    public int turnsCharged = 0;
    public int actionsUsed = 0;
    public int reactionsUsed = 0;
    private BattleEvent chargeMove;
    public int TotalActionsUsed { get { return actionsUsed + reactionsUsed; } }

    public string FullTeamName { get { return team.teamName + " " + hero.characterName; } }
    [SerializeField] List<StatusCondition> statusEffects = new();
    public StatusCondition[] StatusConditions { get { return statusEffects.ToArray(); } }
    public override string ToString()
    {
        string hex = ColorUtility.ToHtmlStringRGB(team.teamColor);
        return $"<color=#{hex}>{RandomNameGenerator.Capitalize(hero.characterName)}</color>";
    }
    public Vector2 placement;
    public Attributes Attributes
    {
        get { return hero.CombinedAttributes * modifiers; }
    }
    public bool IsAlive => hero.health.IsAlive;

    public bool Flying { get { return hero.heroClass.tags.Contains(Enum_HeroClass_Tags.flying); } }

    private void Awake()
    {
        //Initialize();

        AutoBattleManager.OnBattleEvent += CheckStatuses;
        AutoBattleManager.OnBattleEvent += ListenForEvent;
    }
    private void OnDestroy()
    {
        all.Remove(this);

        AutoBattleManager.OnBattleEvent -= CheckStatuses;
        AutoBattleManager.OnBattleEvent -= ListenForEvent;
    } 
    private void OnEnable()
    {
        if (all.Contains(this) == false) 
            all.Add(this); 
    }
    private void Update()
    {
        if (animations)
            animations.animator.SetBool("Alive", IsAlive);
    }

    private void ListenForEvent(BattleEvent action)
    {
        if (action == null)
        {
            Debug.LogWarning($"[{name}] Received NULL BattleEvent.");
            return;
        } 

        if (chargeMove != null) 
            return; // Charging move, cancel reactions

        BattleEvent response = hero.plan.Compare(this, action);

        if (response == null) 
            return;  

        if (response.move.limited && response.move == action.move && response.owner.team == action.owner.team) 
            return; 

        hero.plan.MarkResponded(action.instanceId, this); 

        AutoBattleManager.AddToStack(response);
    }
    private void CheckStatuses(BattleEvent action)
    {
        if (action == null) return;

        if (statusEffects.Count > 0)
            for (int i = statusEffects.Count - 1; i >= 0; i--)
            {
                StatusCondition item = statusEffects[i];
                item.Apply(this, action);
            }

        var passives = hero.ActivePassives;
        if (passives.Count > 0)
            for (int i1 = 0; i1 < passives.Count; i1++)
            {
                Passive item = passives[i1];
                item.ApplyEffects(this, action, 1);
            }

        CleanUpStatuses();
    }
    public void CleanUpStatuses()
    {
        if (statusEffects.Count == 0) return;

        for (int i = statusEffects.Count - 1; i >= 0; i--)
        {
            if (statusEffects[i].duration == 0 || statusEffects[i].stacks == 0) 
                statusEffects.RemoveAt(i); 
        }
    }
    public void RemoveStatusDirectly(int id)
    {
        if (statusEffects.Count == 0) return;

        for (int i = statusEffects.Count - 1; i >= 0; i--)
        { 
            if (statusEffects[i].status.GetID() == id)
                statusEffects.RemoveAt(i);
        }
    }
    public void RemoveStatusDuration(int id)
    {
        for (int i = statusEffects.Count - 1; i <= 0; i--)
        { 
            if (statusEffects[i].status.GetID() == id)
                statusEffects[i].duration = 0;
        } 
    }

    public void Initialize()
    {
        hero.Initialize();
        modifiers = new(1);

        hero.health.shield_current = Attributes.shield;
        actionPoints = Attributes.actions;
        reactionPoints =Attributes.reactions; 

        //Debug.LogWarning($"Spawned {this} : AP = {actionPoints}({Attributes.actions}) " +
        //    $"| {reactionPoints}({Attributes.reactions})");

        FindArt();
        CollectMods();
    }
    public void FindArt(bool flip = true)
    {  
        artRenderer = GetComponentInChildren<SpriteRenderer>();
        if (!artRenderer) return;

        FlipArt(flip);

        if (hero.heroClass && hero.heroClass.actorImage)
        {
            artRenderer.sprite = hero.heroClass.actorImage;

            MaterialPropertyBlock properties = new();
            artRenderer.GetPropertyBlock(properties);

            properties.SetTexture("_MaskTex",
                hero.heroClass.colorMask != null
                    ? hero.heroClass.colorMask
                    : Texture2D.blackTexture);

            properties.SetColor("_TeamColor", team.teamColor);
            properties.SetColor("_ArmorColor", hero.equipment.armor.color);
            properties.SetColor("_WeaponColor", hero.equipment.weapon.color);

            artRenderer.SetPropertyBlock(properties);
        }
    }
    public void FlipArt(bool flip = true)
    {
        if (artRenderer)
            artRenderer.flipX = flip;

        if (particlePoint)
            particlePoint.eulerAngles = flip ? Vector3.zero : new Vector3(0f, 180f, 0f);
    }
    private Coroutine routine;
    private void InitiateMove(BattleEvent battleEvent)
    {
        if (battleEvent == null || routine != null) return;

        if(turnsCharged < battleEvent.move.chargeTurns)
        {
            routine = StartCoroutine(ChargeTurn(battleEvent));
        }
        else
        {
            // No more charge move
            chargeMove = null;
            turnsCharged = 0;

            CollectMods();
            routine = StartCoroutine(HitAllTargets(battleEvent));
        } 
    }
    private List<BattleCombatant> FilterDefeated(List<BattleCombatant> input)
    {
        List<BattleCombatant> living = new();
        foreach(var item in input)
            if(item.IsAlive) living.Add(item);

        return living;
    }

    public delegate void NextAction(BattleEvent battleEvent);
    public NextAction nextAction;
    public void TakeTurn()
    { 
        //Debug.Log($"{team} : {hero} starting turn...");
        if (actionPoints <= 0) return; 
        idled = !PrimaryActionStart();
    }

    public bool PrimaryActionStart()
    { 
        // Plan now owns: choosing a move, filtering targets, applying range extension,
        // and returning a fully-formed BattleEvent (or null).
        BattleEvent action = chargeMove == null ? hero.plan.GetActionEvent(this) : chargeMove;

        if (action == null)
        {
            AutoBattleManager.PushEndTurn();
            if (idled == true) actionPoints = 0;
            return false; // Idled
        }

        PlayMoveParticle(action.move);
        AutoBattleManager.AddToStack(action); 
        return true;
    }
    public void PlayMoveParticle(BattleMove move)
    {
        if (move.moveType == MoveType.special && specialEffect)
            specialEffect.Play();
        else if (move.moveType == MoveType.action && actionEffect)
            actionEffect.Play();
        else if (move.moveType == MoveType.reaction && reactionEffect)
            reactionEffect.Play();
    }
    public void PlayChargingPArticle()
    {
        if (chargingEffect) chargingEffect.Play();
    }

    public void TriggerEvent(BattleEvent action)
    { 
        Debug.LogWarning($"{name} : NEW EVENT = {action.eventType}: " +
            $"Owner = {action.owner} : Source = {action.source} : Targets = {action.targets.Count}");
        //AutoBattleManager.CallEvent(action);
    }
    public void UseActionPoints(int amount)
    { 
        int totalCost = ParseModifiers(Enum_BattleEvent_Modifier.actionCost, null, amount);
        actionPoints = Mathf.Clamp(actionPoints - totalCost, 0, 4);

        actionsUsed++;
    }
    public void UseReactionPoints(int amount)
    {
        int totalCost = ParseModifiers(Enum_BattleEvent_Modifier.reactionCost, null, amount);
        reactionPoints = Mathf.Clamp(reactionPoints - totalCost, 0, 4);

        reactionsUsed++;
    }

    // -------- Event helpers --------
    public void Emit(BattleEvent e)
    {
        if (e == null) return;
        e.owner = this;
        AutoBattleManager.CallEvent(e);
    }

    // Creates a "view" of an existing event from another owner's perspective.
    public BattleEvent AsOwner(BattleEvent e, Enum_BattleEvent_Type newType)
    {
        if (e == null) return null;
        BattleEvent copy = e.Copy(this, newType);
        Emit(copy);

        return copy;
    }
    public BattleEvent AsOwner(BattleEvent e, Enum_BattleEvent_Type newType, float newValue = 0)
    {
        if (e == null) return null;
        BattleEvent copy = e.Copy(this, newType);
        Emit(copy);

        return copy;
    }

    // -------- Turn / action publication --------
    public void UseMove(BattleEvent action)
    {
        if (action == null || action.move == null) return;

        if(MoveFailed(action))
            AutoBattleManager.CompleteAction(action);

        else
        {  
            UI_PopUpTrigger.RigidPopUp(action.move ? action.move.skillName : "no move", transform.position, 64);
            Emit(action);
             
            InitiateMove(action);
        } 
    }
    private bool MoveFailed(BattleEvent action)
    { 
        if (!IsAlive && !action.move.canUseWhileDefeated)
            return true; // failed
         
        if (!action.move.canTargetDefeated)
        {
            action.targets = FilterDefeated(action.targets);
            if (action.targets.Count == 0)
                return true; // failed
        } 

        return false; // success
    }
     
    IEnumerator HitAllTargets(BattleEvent battleEvent)
    {
        Vector3 homePos = transform.position;

        battleEvent.move.UseCost(battleEvent.owner);
        battleEvent.owner.AsOwner(
            battleEvent,
            Enum_BattleEvent_Type.onManaSpent,
            battleEvent.value
        );

        Transform target = battleEvent.targets[0].transform;

        yield return new WaitForSeconds(0.3f); 
        PlayMoveEffect(battleEvent); 
        yield return new WaitForSeconds(0.5f);

        // Select attack type
        if (battleEvent.move.projectile != null && !battleEvent.move.melee)
            yield return StartCoroutine(ProjectileAttack(battleEvent)); // Fires a projectile

        else if (battleEvent.move.melee) // Move to and hit the target
            yield return StartCoroutine(MeleeAttack(battleEvent, homePos, target)); 

        // Spellcasting without projectile
        else yield return StartCoroutine(StandardAttack(battleEvent)); 

        yield return new WaitForSeconds(waitTime);

        routine = null;
        AutoBattleManager.CompleteAction(battleEvent);
    }
    IEnumerator ChargeTurn(BattleEvent battleEvent)
    {
        chargeMove = battleEvent;
        turnsCharged += 1;

        PrintPopUp("Charging", Color.white);
        PlayChargingPArticle();

        yield return new WaitForSeconds(1);

        routine = null;
        AutoBattleManager.CompleteAction(battleEvent, true); // true for silently, no events
    }

    IEnumerator ProjectileAttack(BattleEvent battleEvent)
    {
        BattleMove move = battleEvent.move;
        float maxProjectileDuration = 0f;

        if (battleEvent.targets == null || battleEvent.targets.Count == 0)
            yield break;

        BattleCombatant firstTarget = battleEvent.targets[0];

        // Start effects
        bool started = false;
        PlayParticleTiming(() => started = true, move, firstTarget, BattleParticleTiming.OnStart);
        yield return new WaitUntil(() => started);

        for (int h = 0; h < move.hits; h++)
        {
            // Snapshot targets for this hit cycle
            BattleCombatant[] targets = battleEvent.targets.ToArray();

            foreach (BattleCombatant target in targets)
            {
                if (!target) continue;

                // Attack animation
                PlayAnimation(move.animationID);

                bool animated = false;
                PlayParticleTiming(() => animated = true, move, target, BattleParticleTiming.OnAnimate);
                yield return new WaitUntil(() => animated);

                // Spawn projectile
                var proj = Instantiate(
                    move.projectile,
                    transform.position,
                    transform.rotation
                );

                proj.MoveToAndComplete(target.transform.position, () =>
                {
                    if (!target) return;

                    PlayParticleTiming(() =>
                    {
                        if (target)
                            move.UseMove(battleEvent.source, target);

                    }, move, target, BattleParticleTiming.OnHit);
                });

                if (proj.duration > maxProjectileDuration)
                    maxProjectileDuration = proj.duration;

                yield return new WaitForSeconds(
                    settleTime / (float)targets.Length
                );
            }
        }

        // Wait for projectiles to finish
        yield return new WaitForSeconds(maxProjectileDuration);

        // Completion effects
        bool completed = false;
        PlayParticleTiming(() => completed = true, move, firstTarget, BattleParticleTiming.OnComplete);
        yield return new WaitUntil(() => completed);
    }

    IEnumerator MeleeAttack(BattleEvent battleEvent, Vector3 homePos, Transform target)
    {
        BattleMove move = battleEvent.move;

        if (battleEvent.targets == null || battleEvent.targets.Count == 0)
            yield break;

        BattleCombatant firstTarget = battleEvent.targets[0];

        // Start effects
        bool started = false;
        PlayParticleTiming(() => started = true, move, firstTarget, BattleParticleTiming.OnStart);
        yield return new WaitUntil(() => started);

        // Leap toward target
        PlayAnimation("leap");

        LeanTween.move(
            gameObject,
            target.position + target.forward * travelOffset,
            walkTime
        ).setEaseInOutSine();

        yield return new WaitForSeconds(walkTime);

        // Attack animation effects
        for (int h = 0; h < move.hits; h++)
        {
            PlayAnimation(move.animationID);

            bool animated = false;
            PlayParticleTiming(() => animated = true, move, firstTarget, BattleParticleTiming.OnAnimate);
            yield return new WaitUntil(() => animated);

            // Snapshot targets for this hit cycle
            BattleCombatant[] targets = battleEvent.targets.ToArray();

            foreach (BattleCombatant combatant in targets)
            {
                if (!combatant) continue;

                bool hitComplete = false;

                PlayParticleTiming(() =>
                {
                    if (combatant)
                        move.UseMove(battleEvent.source, combatant);

                    hitComplete = true;

                }, move, combatant, BattleParticleTiming.OnHit);

                yield return new WaitUntil(() => hitComplete);

                yield return new WaitForSeconds(
                    settleTime / (float)targets.Length * 0.7f
                );
            }
        }

        // Return home
        yield return new WaitForSeconds(settleTime * 0.5f);

        LeanTween.move(gameObject, homePos, walkTime)
            .setEaseInOutSine();

        yield return new WaitForSeconds(walkTime);

        // Completion effects
        bool completed = false;
        PlayParticleTiming(() => completed = true, move, firstTarget, BattleParticleTiming.OnComplete);
        yield return new WaitUntil(() => completed);
    }

    IEnumerator StandardAttack(BattleEvent battleEvent)
    {
        BattleMove move = battleEvent.move;

        if (battleEvent.targets == null || battleEvent.targets.Count == 0)
            yield break;

        BattleCombatant firstTarget = battleEvent.targets[0];

        // Start effects
        bool started = false;
        PlayParticleTiming(() => started = true, move, firstTarget, BattleParticleTiming.OnStart);
        yield return new WaitUntil(() => started);

        // Attack animation
        PlayAnimation(move.animationID);

        bool animated = false;
        PlayParticleTiming(() => animated = true, move, firstTarget, BattleParticleTiming.OnAnimate);
        yield return new WaitUntil(() => animated);

        yield return new WaitForSeconds(walkTime);

        // Apply hits
        for (int h = 0; h < move.hits; h++)
        {
            // Snapshot targets for this hit cycle
            BattleCombatant[] targets = battleEvent.targets.ToArray();

            foreach (BattleCombatant target in targets)
            {
                if (!target) continue;

                bool hitComplete = false;

                PlayParticleTiming(() =>
                {
                    if (target)
                        move.UseMove(battleEvent.source, target);

                    hitComplete = true;

                }, move, target, BattleParticleTiming.OnHit);

                yield return new WaitUntil(() => hitComplete);

                yield return new WaitForSeconds(
                    settleTime / (float)targets.Length
                );
            }
        }

        // Completion effects
        bool completed = false;
        PlayParticleTiming(() => completed = true, move, firstTarget, BattleParticleTiming.OnComplete);
        yield return new WaitUntil(() => completed);
    }

    void PlayMoveEffect(BattleEvent battleEvent)
    {
        if (specialEffect != null && battleEvent.move.moveType == MoveType.special)
        {
            specialEffect.Play(); 
        }
        else if (reactionEffect != null && battleEvent.move.moveType == MoveType.reaction)
        {
            reactionEffect.Play(); 
        }
    }

    // -------- Damage / healing publication --------
    public void TakeDamage(BattleEvent battleEvent)
    {
        if (battleEvent == null) return;

        float startingHealth = hero.health.health_current;
        float startingShield = hero.health.shield_current;

        battleEvent.eventType = Enum_BattleEvent_Type.onDamageTaken;
        battleEvent.owner = this;

        battleEvent.value = ParseModifiers(Enum_BattleEvent_Modifier.damageIn, battleEvent, battleEvent.value);
        battleEvent.value = battleEvent.source.ParseModifiers(Enum_BattleEvent_Modifier.damageOut, battleEvent, battleEvent.value);

        hero.health.DamageHealth(battleEvent);

        battleEvent.source.AsOwner(battleEvent,Enum_BattleEvent_Type.onHit); 
        DamageShieldEvent(startingShield, battleEvent);
        DamageHealthEvent(startingHealth, battleEvent);
    }
    public void LoseHealth(BattleEvent battleEvent)
    {
        if (battleEvent == null) return;

        float startingHealth = hero.health.health_current; 

        battleEvent.eventType = Enum_BattleEvent_Type.onHealthLost;
        battleEvent.owner = this;

        hero.health.LoseHealth(battleEvent); 
        LoseHealthEvent(startingHealth, battleEvent);
    }
    public void RaiseMana(BattleEvent battleEvent)
    {
        if (battleEvent == null) return;

        float startingHealth = hero.health.health_current;

        battleEvent.eventType = Enum_BattleEvent_Type.onHealthLost;
        battleEvent.owner = this;

        hero.health.LoseHealth(battleEvent);
        LoseHealthEvent(startingHealth, battleEvent);
    }
    public void DamageMana(BattleEvent battleEvent)
    {
        if (battleEvent == null) return;

        float startingHealth = hero.health.health_current;

        battleEvent.eventType = Enum_BattleEvent_Type.onManaLoss;
        battleEvent.owner = this;

        hero.health.DrainMana(battleEvent);
        Emit(battleEvent);
    }
    public void DamageShieldEvent(float startingShield, BattleEvent battleEvent)
    { 
        if (startingShield > 0 && hero.health.shield_current < startingShield)
        {
            var shieldEvent = battleEvent.Copy(Enum_BattleEvent_Type.onShieldDamaged);
            float result = Mathf.CeilToInt(startingShield - hero.health.shield_current);
            shieldEvent.value = result;

            Emit(shieldEvent);
            battleEvent.source.AsOwner(battleEvent, Enum_BattleEvent_Type.onDamageBlocked); 

            if (hero.health.shield_current <= 0)
                Emit(shieldEvent.Copy(Enum_BattleEvent_Type.onShieldBroken));

            else PlayAnimation("block");

            UI_PopUpTrigger.CreatePopUp((-result).ToString(), Color.darkCyan, transform.position);
        }
    }
    public void DamageHealthEvent(float startingHealth, BattleEvent battleEvent)
    {
        if (hero.health.health_current < startingHealth)
        {  
            Emit(battleEvent); // Should be On Damage Taken
            AsOwner(battleEvent, Enum_BattleEvent_Type.onHealthLost);
            battleEvent.source.AsOwner(battleEvent, Enum_BattleEvent_Type.onDamageDealt);
             
            battleEvent.source.hero.health.mana_current += 
                battleEvent.source.ParseModifiers(
                    Enum_BattleEvent_Modifier.manaIn, 
                    battleEvent, 
                    battleEvent.value / hero.health.health_max * 25f);

            if (IsAlive == false)
                NotifyDefeated(battleEvent);

            else PlayAnimation("hurt");

            UI_PopUpTrigger.CreatePopUp(battleEvent.value.ToString("0"), Color.red, transform.position);
        }
    }
    public void LoseHealthEvent(float startingHealth, BattleEvent battleEvent)
    {
        if (hero.health.health_current < startingHealth)
        {
            battleEvent.eventType = Enum_BattleEvent_Type.onHealthLost;
            battleEvent.owner = this;

            Emit(battleEvent); 

            if (IsAlive == false)
                NotifyDefeated(battleEvent);

            else PlayAnimation("hurt");

            UI_PopUpTrigger.CreatePopUp("-" + battleEvent.value.ToString(), Color.purple, transform.position);
        }
    }

    public void GainHealth(BattleEvent battleEvent)
    {
        if (battleEvent == null) return;

        battleEvent.eventType = Enum_BattleEvent_Type.onHealthGained;
        battleEvent.owner = this;

        battleEvent.value = ParseModifiers(Enum_BattleEvent_Modifier.healingIn, battleEvent, battleEvent.value);
        battleEvent.value = battleEvent.source.ParseModifiers(Enum_BattleEvent_Modifier.healingOut, battleEvent, battleEvent.value);
         
        hero.health.GainHealth(battleEvent);
        if (hero.health.IsDead)
            NotifyRevived(battleEvent);

        Emit(battleEvent);
        UI_PopUpTrigger.CreatePopUp(Mathf.CeilToInt(battleEvent.value).ToString(), Color.green, transform.position);
    }
    public void GainShield(BattleEvent battleEvent)
    {
        if (battleEvent == null) return;

        float startSH = hero.health.shield_current;

        battleEvent.eventType = Enum_BattleEvent_Type.onShieldGained;
        battleEvent.owner = this;

        hero.health.GainShield(battleEvent);

        float gained = Mathf.CeilToInt(hero.health.shield_current - startSH);
        if (gained > 0f)
            UI_PopUpTrigger.CreatePopUp(gained.ToString("0"), Color.cyan, transform.position);

        Emit(battleEvent);
    }
    public void ShatterShield(BattleEvent battleEvent)
    {
        if (battleEvent == null || hero.health.shield_current <= 0) return;

        float startSH = hero.health.shield_current;

        battleEvent.eventType = Enum_BattleEvent_Type.onShieldDamaged;
        battleEvent.owner = this;

        // Shatter magnitude is positive
        hero.health.shield_current -= Mathf.Min(hero.health.shield_current, battleEvent.value);
        hero.health.Clamp();

        float shattered = Mathf.CeilToInt(startSH - hero.health.shield_current);
        if (shattered > 0f)
            UI_PopUpTrigger.CreatePopUp("-" + shattered.ToString("0"), Color.orange, transform.position);

        Emit(battleEvent);

        if (hero.health.shield_current <= 0f)
            Emit(battleEvent.Copy(Enum_BattleEvent_Type.onShieldBroken));
    }


    // Call these from wherever your health component knows the result.
    public void NotifyDefeated(BattleEvent battleEvent = null)
    {  
        battleEvent.source.AsOwner(battleEvent, Enum_BattleEvent_Type.onDefeated);
        UI_PopUpTrigger.CreatePopUp("Defeat", Color.maroon, transform.position, 48);

        //animations.animator.SetBool("Alive", false); 
    }

    public void NotifyRevived(BattleEvent battleEvent = null)
    {  
        battleEvent.source.AsOwner(battleEvent, Enum_BattleEvent_Type.onRevived);
        UI_PopUpTrigger.CreatePopUp("Revive", Color.wheat, transform.position, 48);

        //animations.animator.SetBool("Alive", true);
    }
    public void EvadeAttack(BattleEvent battleEvent = null)
    {  
        battleEvent.source.AsOwner(battleEvent, Enum_BattleEvent_Type.onMoveEvaded);
        UI_PopUpTrigger.CreatePopUp("Miss", Color.white, transform.position, 48);
        animations.PlayAnimation("evade");
    }
    public void MissAttack(BattleEvent battleEvent = null)
    {  
        battleEvent.source.AsOwner(battleEvent, Enum_BattleEvent_Type.onMoveMissed);
    }

    // -------- Turn boundary publication (optional but clean) --------
    public void NotifyTurnStart()
    {
        hero.plan.ResetResponseLedger();
        Emit(new BattleEvent()
        {
            eventType = Enum_BattleEvent_Type.onTurnStart,
            owner = this,
            source = this,
            targets = new() { this },
            value = 0,
        });
    }

    public void NotifyTurnEnd()
    {
        Emit(new BattleEvent()
        {
            eventType = Enum_BattleEvent_Type.onTurnEnd,
            owner = this,
            source = this,
            targets = new() { this },
            value = 0,
        });
    }

    public void PlayAnimation(string id)
    {
        if (!animations) return;
        animations.PlayAnimation(id);
    }

    public static void PlayGlobalEvent(Enum_BattleEvent_Type eventType)
    {
        //Debug.LogWarning($"Playing global event of: {eventType} to {AllCombatants.Length} heroes");

        foreach(var hero in AllCombatants)
        { 
            if(hero.IsAlive)
            {
                hero.Emit(new()
                {
                    eventType = eventType, 
                    owner = hero,
                    source = hero,
                    targets = new() { hero },
                    value = -1,
                });
            }
        }
    }

    public bool HasStatus(int id)
    {
        if (statusEffects.Count == 0)
            return false;

        foreach (var item in statusEffects)
            if (item.status.GetID() == id)
                return true;

        return false;
    }
    public void GainStatus(BattleCombatant source, StatusCondition status)
    {
        if (status == null) return;
        if (statusEffects.Contains(status))
            return; // Checks if we already have this same instance of status condition

        Debug.Log($"{this} : Attempting to place {status.status.skillName} onto {hero}");

        BattleEvent e = new()
        {
            eventType = Enum_BattleEvent_Type.onStatusApplied,
            owner = this,
            source = source,
            targets = new() { this },
            value = status.stacks,
        };

        if (CheckExisting(e, status) == true)
            return; // If we already have the condition, add to stacks or duration as needed
        
        // Otherwise we add the new status to our list
        statusEffects.Add(status); 

        PrintPopUp(status.status.skillName, status.status.color);
        Emit(e);

        CollectMods();
    }
    private bool CheckExisting(BattleEvent e, StatusCondition status)
    {
        for (int i = 0; i < statusEffects.Count; i++)
        { // Check for if the effect currently exists
            if (statusEffects[0].status == status.status)
            {
                statusEffects[0] = statusEffects[0] + status;
                e.value = statusEffects[0].stacks;

                PrintPopUp(status.status.skillName, status.status.color);
                Emit(e);

                CollectMods();
                Debug.Log("EFFECT EXISTS and Extended");
                return true;
            }
        }

        return false;
    }
    public void ClearAllStatusConditions()
    {
        statusEffects.Clear();
        CollectMods();
    }

    public void PrintPopUp(string text, Color color, float size = 64)
    {
        UI_PopUpTrigger.CreatePopUp(text, color, transform.position, size);
    }

    readonly List<Modifier> mods = new();
    public float ParseModifiers(Enum_BattleEvent_Modifier type, BattleEvent action, float input)
    {
        // CollectMods();
        float result = input;

        foreach (var item in mods)
            result = item.Parse(type, this, action, result);

        return result;
    }
    public int ParseModifiers(Enum_BattleEvent_Modifier type, BattleEvent action, int input)
    {
        CollectMods();
        float result = input;
        foreach (var item in mods)
            result = item.Parse(type, this, action, result);

        return (int)result;
    }
    private void CollectMods()
    {
        mods.Clear();

        foreach (var effect in statusEffects)
            mods.AddRange(effect.status.modifiers);

        mods.AddRange(hero.equipment.GetAllMods()); 

        mods.Sort((a, b) => a.priority.CompareTo(b.priority));
    }
    public float Initiative()
    {
        CollectMods();

        float speed = Attributes.speed;

        speed = ParseModifiers(Enum_BattleEvent_Modifier.initiative, null, speed);

        return speed;
    }

    public BattleCombatant[] GetAllAllies()
    {
        List<BattleCombatant> allied = new();
        foreach (var item in AllCombatants)
            if (item.team == team)
                allied.Add(item);

        return allied.ToArray();
    }
    public BattleCombatant[] GetAllEnemies()
    {
        List<BattleCombatant> enemy = new();
        foreach (var item in AllCombatants)
            if (item.team != team)
                enemy.Add(item);

        return enemy.ToArray();
    }

    public void GainMana(float mana, BattleEvent e)
    {
        if (isMinion) hero.health.mana_current = 0;

        if (IsAlive == false || isMinion == true)
            return;

        if (e.move != null && e.move.moveType == MoveType.special)
            return;

        mana = ParseModifiers(Enum_BattleEvent_Modifier.manaIn, e, mana);
        hero.health.GainMana(mana);

        AsOwner(e, Enum_BattleEvent_Type.onManaGained, mana);
    }

    public void GainMana(BattleEvent e)
    {
        if (IsAlive == false)
            return;

        if (e.move && e.move.moveType == MoveType.special)
            return;

        e.value = ParseModifiers(Enum_BattleEvent_Modifier.manaIn, e, e.value);
        hero.health.GainMana(e.value);
    }

    public void ResetMana()
    {
        hero.health.mana_current = 0; 
    }

    public void SpendAP_RP(BattleMove move)
    {
        if (!move) return;

        move.UseCost(this);
    }


    public bool IsFrontRow() => placement.x <= 0;
    public bool IsBackRow() => placement.x > 0;

    public static List<BattleCombatant> AllAlliesOf(BattleCombatant owner)
    {
        List<BattleCombatant> list = new();
        foreach (var item in all)
            if (item.team == owner.team)
                list.Add(item);

        return list;
    }

    public static List<BattleCombatant> AllEnemiesOf(BattleCombatant owner)
    {
        List<BattleCombatant> list = new();
        foreach (var item in all)
            if (item.team != owner.team)
                list.Add(item);

        return list;
    }

    public void PlayParticleTiming(Action action, BattleMove move, BattleCombatant target, BattleParticleTiming timing)
    {
        var hits = GetParticles(move, timing);

        // No particles? Execute the callback immediately.
        if (hits == null || hits.Length == 0)
        {
            action?.Invoke();
            return;
        }

        for (int i = 0; i < hits.Length; i++)
        { 
            var particle = Instantiate( hits[i], GetParticlePosition(target, hits[i]), 
                particlePoint ? particlePoint.rotation : Quaternion.identity);

            // Only the first particle receives the callback.
            particle.Play(i == 0 ? action : null);
        }
    }

    public BattleParticleController[] GetParticles(BattleMove move, BattleParticleTiming timing)
    {
        if (move == null || move.specialEffects == null)
            return Array.Empty<BattleParticleController>();

        List<BattleParticleController> results = new();

        foreach (BattleParticleController particle in move.specialEffects)
        {
            if (particle != null && particle.timing == timing)
                results.Add(particle);
        }

        return results.ToArray();
    }

    public Vector3 GetParticlePosition(BattleCombatant target, BattleParticleController particle)
    {
        return particle.target switch
        {
            BattleParticleTarget.Source => transform.position,
            BattleParticleTarget.Unit => target ? target.parent.transform.position : parent.transform.position,
            BattleParticleTarget.Field => Vector3.zero,
            _ => target ? target.transform.position : transform.position,
        };
    }
}
