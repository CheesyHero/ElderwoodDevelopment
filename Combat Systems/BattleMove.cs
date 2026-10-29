using System.Collections.Generic; 
using UnityEngine; 

public enum DamageTypes
{
    physical, magical, pure
} 
public enum RangeExtension
{
    none, row, column
}
public enum MoveType
{
    action = 1, reaction = 2, special = 0
}
public enum DesiredTargetTeam
{
    allies, enemies, any
}
public enum TargetTypes
{
    singleTarget, self, all, other
}
[CreateAssetMenu(fileName = "New Move", menuName = "NMD/RPG/Battle Move", order = 1)]
public class BattleMove : Skill
{ 
    public int cost = 1;
    public bool melee;
    public MoveType moveType = MoveType.action; 
    public DamageTypes damageType = DamageTypes.physical;
    public DesiredTargetTeam team = DesiredTargetTeam.enemies;
    public TargetTypes targetType = TargetTypes.singleTarget;
    public RangeExtension range = RangeExtension.none;
    public bool canTargetDefeated = false;
    public bool canUseWhileDefeated = false;
    public bool limited = false; // only one allied hero can have a move of this name on the stack
    [Range(0, 3)]
    public int chargeTurns = 0;

    [Header("Values")]
    [Range(1, 5)]
    public int hits = 1;
    [Range(-1, 100)]
    public float accuracy = 80f;
    [Range(-1, 100)]
    public float criticalChance = -1f;
    [Range(1.1f, 5)]
    public float criticalMult = 1.5f;
    public BattleEffect[] effects;
    public TriggerCondition[] triggers;

    [Header("Animation")]
    public string animationID = "attack";
    public Projectile projectile;
    public ParticleSystem impactFX;
    public ParticleAnimator[] animations;

    [Space(8)]
    public BattleParticleController[] specialEffects;

    [Header("Battle Plan")]
    public FilterRule[] defaultRules;
    public FilterRule[] recommendedRules;

    public override string PrintTooltip()
    {
        string acc = accuracy < 0 ? "Always Hit" : $"{accuracy}%";
        string crit = criticalChance < 0 ? "None" : $"{criticalChance}% x{criticalMult}";
        string meleeTxt = melee ? "Melee" : "Ranged";
        string skillnameprint = "<u>";

        if (moveType == MoveType.action)
            skillnameprint += ($"<b><color=#ed9f40>{skillName}</b></color>: ");
        else if (moveType == MoveType.reaction)
            skillnameprint += ($"<b><color=#40b6ed>{skillName}</b></color>: ");
        else // special
            skillnameprint += ($"<b><color=#9c40ed>{skillName}</b></color>: ");


        return
            $"{skillnameprint} {meleeTxt} {damageType} {moveType}</u>\n" +
            $"{description}\n\n" +

            $"<color=#7abaff><b>Cost</color></b>: {cost} {moveType}\n" + 
            $"<color=#7abaff><b>Target</color></b>: {team}, {targetType}\n" +
            $"<color=#7abaff><b>Range</color></b>: {range}\n" +
            $"<color=#7abaff><b>Accuracy</color></b>: {acc}\n" +
            $"<color=#7abaff><b>Critical</color></b>: {crit}\n";
    }
    public override string ToString()
    { return skillName; } 
    public BattleEvent CreateEvent(BattleCombatant attacker, BattleCombatant defenders)
    {
        return new(this, attacker, attacker, defenders, effects.Length);
    }
    public BattleEvent CreateEvent(BattleCombatant attacker, List<BattleCombatant> defenders)
    {
        BattleEvent next = new()
        {
            instanceId = 1,
            move = this,
            eventType = moveType == MoveType.reaction ? Enum_BattleEvent_Type.onReactionUsed : Enum_BattleEvent_Type.onMoveUsed,
            owner = attacker,
            source = attacker,
            targets = targetType == TargetTypes.self ? new() { attacker } : defenders,
            value = defenders.Count,
        };

        //Debug.Log($"BATTLE MOVE: New event created {next.eventType} : {next.owner} : " +
        //    $"{(next.move != null ? next.move.moveName : "No Move")}");
        return next;
    }

    static BattleEvent currentAction;
    public void UseMove(BattleCombatant attacker, BattleCombatant defender)
    {
        if (defender.IsAlive == false && canTargetDefeated == false)
            return;
        if (attacker.IsAlive == false && canUseWhileDefeated == false)
            return;

        currentAction = CreateEvent(attacker, defender); 
        if (RollHitDice(attacker, defender, currentAction) == false)
        {
            attacker.MissAttack(currentAction);
            defender.EvadeAttack(currentAction);
        }

        else
        {
            float critical = GetCritical(attacker, defender);
            if (impactFX) Instantiate(impactFX, defender.transform.position + Vector3.up, defender.transform.rotation); 

            foreach (var effect in effects)  
                effect.Parse(CreateEvent(attacker, defender), critical); 

            if (critical != 1)
                UI_PopUpTrigger.CreatePopUp("CRIT!", Color.yellowNice, defender.transform.position, 48);

            attacker.GainMana(10, currentAction);
        }
    }
    public float GetCritical(BattleCombatant attacker, BattleCombatant defender)
    {
        if (criticalChance <= 0)
            return 1f;

        int roll = Random.Range(0, 99);
        float target = attacker.Attributes.criticalChance;
        target = attacker.ParseModifiers(Enum_BattleEvent_Modifier.criticalChance, currentAction, target);  
        target = defender.ParseModifiers(Enum_BattleEvent_Modifier.criticalEvade, currentAction, target);  

        float result = attacker.Attributes.criticalDamage;
        target = attacker.ParseModifiers(Enum_BattleEvent_Modifier.criticalDamage, currentAction, target);
        target = defender.ParseModifiers(Enum_BattleEvent_Modifier.criticalResist, currentAction, target);

        if (roll <= criticalChance + target)
            return criticalMult * (1 + result / 100f);

        return 1f;
    }

    public bool RollHitDice(BattleCombatant attacker, BattleCombatant defender, BattleEvent action)
    {
        if (accuracy <= 0) return true;

        float offense = GetAttribute(attacker, Enum_HeroAttributes.accuracy);
        float defense = GetAttribute(defender, Enum_HeroAttributes.evasion);
        BattleEffect.Equalize(ref offense, ref defense);

        float hitChance = accuracy * (offense / defense);

        hitChance = attacker.ParseModifiers(Enum_BattleEvent_Modifier.hitChance, action, hitChance);
        //Debug.Log($"Testing hitChance on ATTACKER.hitChance = {hitChance}");
        hitChance = defender.ParseModifiers(Enum_BattleEvent_Modifier.dodgeChance, action, hitChance);
        //Debug.Log($"Testing hitChance on DEFENDER.hitChance = {hitChance}");

        if (melee && defender.Flying && !attacker.Flying)
            hitChance *= 0.5f; // Grounded heroes (no flying tag) against flying heroes is 50% less accurate

        else if (!melee && defender.Flying) hitChance *= 1.5f;
        // ranged heroes against flying heroes, regardless of grounding, are more accurate against flying

        float roll = Random.Range(1, 100);
        return roll <= hitChance; // For modifying hit chance: reducing chance inversely increases success
    }
    public float GetAttribute(BattleCombatant source, Enum_HeroAttributes target)
    {
        return source ? Mathf.Max(source.Attributes.GetStat(target), 1) : 1;
    }
    public List<BattleCombatant> FilterByPlayerFilters(
        BattleCombatant checker,
        FilterRule[] filters,
        List<BattleCombatant> list)
    {
        if (list == null || list.Count == 0)
            return new List<BattleCombatant>();

        if (filters == null || filters.Length == 0)
            return list;

        List<BattleCombatant> working = new(list);

        // PASS 1: Apply all filters first
        foreach (var rule in filters)
        {
            if (rule == null || rule.mode != E_MoveFilterMode.filter)
                continue;

            working = rule.Filter(checker, working);

            if (working == null || working.Count == 0)
                return new List<BattleCombatant>();
        }

        // PASS 2: Apply all pickers second
        foreach (var rule in filters)
        {
            if (rule == null || rule.mode != E_MoveFilterMode.picker)
                continue;

            working = rule.Filter(checker, working);

            if (working == null || working.Count == 0)
                return new List<BattleCombatant>();

            // Pickers always return exactly one target.
            if (working.Count > 1)
            {
                int index = UnityEngine.Random.Range(0, working.Count);

                working = new List<BattleCombatant>()
            {
                working[index]
            };
            }
        }

        return working;
    }

    public List<BattleCombatant> FilterByTargetType(BattleCombatant checker, List<BattleCombatant> list)
    {
        List<BattleCombatant> original = new(list.ToArray());
        if (melee) list = FilterByMeleeRange(list);

        return targetType switch
        {
            TargetTypes.self => new List<BattleCombatant>() { checker },
            TargetTypes.all => original,

            // NEW: "other" behaves like singleTarget selection, but never includes the checker
            TargetTypes.other => ExcludeSelf(checker, list),

            // single target
            _ => list,
        };
    }
    static List<BattleCombatant> ExcludeSelf(BattleCombatant checker, List<BattleCombatant> list)
    {
        if (list == null) return new List<BattleCombatant>();
        list.RemoveAll(x => x == null || x == checker);
        return list;
    }
    public List<BattleCombatant> ExtendToRow(BattleCombatant target, List<BattleCombatant> list)
    {
        if (target == null || list == null) return new List<BattleCombatant>();

        int row = Mathf.RoundToInt(target.placement.x);
        List<BattleCombatant> results = new();

        foreach (var item in list)
        {
            if (item == null) continue;
            if (!item.hero.health.IsAlive) continue;

            if (Mathf.RoundToInt(item.placement.x) == row)
                results.Add(item);
        }

        if (!results.Contains(target)) results.Add(target);
        return results;
    }

    public List<BattleCombatant> ExtendToColumn(BattleCombatant target, List<BattleCombatant> fullPool)
    {
        List<BattleCombatant> results = new();

        if (target == null || fullPool == null)
            return results;

        int col = Mathf.RoundToInt(target.placement.y); 

        foreach (BattleCombatant hero in fullPool)
        {
            if (hero == null || !hero.IsAlive)
                continue;

            bool sameColumn =
                Mathf.RoundToInt(hero.placement.y) == col;

            // Standard column targeting.
            if (sameColumn)
            {
                results.Add(hero);
                continue;
            } 
        }

        if (!results.Contains(target))
            results.Add(target);

        return results;
    }

    public List<BattleCombatant> FilterByMeleeRange(List<BattleCombatant> list)
    {
        if (!melee || list == null || list.Count == 0)
            return list;

        int lowestRow = int.MaxValue;

        // Find the lowest occupied row.
        foreach (var item in list)
        {
            if (item == null || !item.IsAlive)
                continue;

            int row = Mathf.RoundToInt(item.placement.x);

            if (row < lowestRow)
                lowestRow = row;
        }

        List<BattleCombatant> results = new();

        // Keep only combatants in the lowest row.
        foreach (var item in list)
        {
            if (item == null || !item.IsAlive)
                continue;

            if (Mathf.RoundToInt(item.placement.x) == lowestRow)
                results.Add(item);
        }

        return results;
    }

    public List<BattleCombatant> GetPreferedTeam(Team team)
    {
        return this.team switch
        {
            DesiredTargetTeam.allies => new List<BattleCombatant>(GetAllAllies(team)),
            DesiredTargetTeam.any => new List<BattleCombatant>(BattleCombatant.AllCombatants),
            //enemies
            _ => new List<BattleCombatant>(GetAllEnemies(team)),
        };
    }

    public BattleCombatant[] GetAllAllies(Team team)
    {
        List<BattleCombatant> valids = new();
        foreach(var hero in BattleCombatant.AllCombatants)
        {
            if(team.CompareAllied(hero.team))
                valids.Add(hero);
        }

        return valids.ToArray();
    }

    public List<BattleCombatant> GetAllEnemies(Team team)
    {
        List<BattleCombatant> valids = new();
        foreach(var hero in BattleCombatant.AllCombatants)
        {
            if(hero.hero.health.IsAlive && team.CompareEnemy(hero.team))
                valids.Add(hero);
        }

        return valids;
    }

    public List<BattleCombatant> PickOneRandomIfSingle(List<BattleCombatant> results)
    {
        if (results == null) return new();

        // NEW: treat "other" as a single-target selection type
        if (targetType != TargetTypes.singleTarget && targetType != TargetTypes.other)
            return results;

        if (results.Count <= 1) return results;

        int i = UnityEngine.Random.Range(0, results.Count);
        return new() { results[i] };
    }
    public bool CanCast(BattleCombatant combatant)
    {
        // no user
        if (!combatant) return false;

        // safeguard to prevent the user from using a move if they were defeated before using it
        if(canUseWhileDefeated == false && combatant.IsAlive == false)
            return false;

        int totalCost = moveType == MoveType.reaction ? 
            combatant.ParseModifiers(Enum_BattleEvent_Modifier.reactionCost, null, cost) : 
            combatant.ParseModifiers(Enum_BattleEvent_Modifier.actionCost, null, cost);

        // Check is special move is available
        if (moveType == MoveType.special)
            return combatant.actionPoints >= totalCost && combatant.hero.health.ManaPercent >= 1f;

        // Use reaction points
        else if (moveType == MoveType.reaction)
            return combatant.reactionPoints >= totalCost;

        // Regular action
        else return combatant.actionPoints >= totalCost; 
    } 
    public void UseCost(BattleCombatant combatant)
    {
        if (!combatant) return;

        if (moveType == MoveType.special)
            combatant.ResetMana();

        if (moveType == MoveType.reaction)
            combatant.UseReactionPoints(cost);

        // Special moves use regular action points
        else combatant.UseActionPoints(cost);
    }

    public BattleEvent GetReaction(BattleCombatant checker, BattleEvent action, FilterRule[] filters)
    {
        if (triggers == null || triggers.Length == 0) return null;
        if (!CanCast(checker)) return null;

        foreach (var item in triggers)
        {
            if (item == null) continue;

            if (item.TryResolveTargets(checker, this, action, filters, out List<BattleCombatant> results))
            {
                if (results != null && results.Count > 0)
                {
                    results = FilterByPlayerFilters(checker, filters, new List<BattleCombatant>(results));
                    if (results.Count == 0) continue;

                    // CRITICAL: single target reactions must pick ONE if trigger returned multiple targets
                    results = PickOneRandomIfSingle(results);
                    if (results.Count == 0) continue;

                    return CreateEvent(checker, results);
                }
            }
        }

        return null;
    }
    public List<BattleCombatant> ResolveTargets(BattleCombatant checker, FilterRule[] filters)
    {
        if (checker == null)
            return new();

        List<BattleCombatant> fullPool = GetPreferedTeam(checker.team);

        Debug.LogWarning(
            $"[TARGET RESOLVE] Move: {skillName} | " +
            $"User: {checker} | " +
            $"Team Setting: {team} | " +
            $"Full Pool: {fullPool.Count} | " +
            $"State: {AutoBattleManager.Manager.GetCurrentStateID()}"
        );

        foreach (var item in fullPool)
            Debug.LogWarning($"[TARGET FULL POOL] {item}");

        if (fullPool == null || fullPool.Count == 0)
            return new();

        List<BattleCombatant> candidatePool =
            melee
                ? FilterByMeleeRange(new List<BattleCombatant>(fullPool))
                : new List<BattleCombatant>(fullPool);

        Debug.LogWarning(
            $"[TARGET CANDIDATES BEFORE TYPE] Move: {skillName} | " +
            $"Count: {candidatePool.Count}"
        );

        List<BattleCombatant> candidates = targetType switch
        {
            TargetTypes.self => new List<BattleCombatant>() { checker },
            TargetTypes.all => new List<BattleCombatant>(fullPool),
            TargetTypes.other => ExcludeSelf(checker, new List<BattleCombatant>(candidatePool)),
            _ => new List<BattleCombatant>(candidatePool)
        };

        Debug.LogWarning(
            $"[TARGET BEFORE FILTERS] Move: {skillName} | " +
            $"Count: {candidates.Count}"
        );

        candidates = FilterByPlayerFilters(checker, filters, candidates);

        Debug.LogWarning(
            $"[TARGET AFTER FILTERS] Move: {skillName} | " +
            $"Count: {(candidates != null ? candidates.Count : 0)}"
        );

        if (candidates == null || candidates.Count == 0)
            return new();

        if (targetType == TargetTypes.singleTarget || targetType == TargetTypes.other)
        {
            int idx = UnityEngine.Random.Range(0, candidates.Count);
            BattleCombatant primary = candidates[idx];

            Debug.LogWarning(
                $"[TARGET SELECTED] Move: {skillName} | " +
                $"Primary: {primary}"
            );

            return ApplyRangeExtension(primary, fullPool);
        }

        if (targetType == TargetTypes.self)
            return ApplyRangeExtension(checker, fullPool);

        return candidates;
    }
    List<BattleCombatant> ApplyRangeExtension(BattleCombatant primary, List<BattleCombatant> fullPool)
    {
        if (primary == null) return new List<BattleCombatant>();

        return range switch
        {
            RangeExtension.row => ExtendToRow(primary, fullPool),
            RangeExtension.column => ExtendToColumn(primary, fullPool),
            _ => new List<BattleCombatant>() { primary },
        };
    }
    public bool HasEffect(Enum_BattleEffect_Effects check)
    {
        foreach(var item in effects)
            if(item.effect == check) 
                return true;

        return false;
    }
}
