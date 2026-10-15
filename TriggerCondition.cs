// TriggerCondition.cs
using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TriggerCondition
{
    public Enum_BattleEvent_Type requiredEventType;
    public Enum_BattleCondition_Targets targetSelector = Enum_BattleCondition_Targets.owner;
     
    public bool excludeDefeatedTargets = true;  
    public BattleCondition[] conditions;

    public bool Matches(BattleCombatant checker, BattleEvent e)
    {
        if (e == null) return false;

        checker = checker != null ? checker : e.owner;

        if (e.eventType != requiredEventType)
            return false;

        if (conditions == null || conditions.Length == 0)
            return true;

        for (int i = 0; i < conditions.Length; i++)
        {
            var c = conditions[i];
            if (c == null) continue;
            if (!c.RunCondition(checker, e)) return false;
        }
        return true;
    }

    // ----------------------------
    // Target resolution entrypoints
    // ----------------------------

    public bool TryResolveTargets(BattleCombatant checker, BattleEvent e, out List<BattleCombatant> resolvedTargets)
    {
        resolvedTargets = new();
        if (!Matches(checker, e)) return false;

        resolvedTargets = ApplyTargetExclusions(ResolveTargets(checker, e, targetSelector));
        return resolvedTargets != null && resolvedTargets.Count > 0;
    }

    public bool TryResolveTargets(
        BattleCombatant checker,
        BattleMove reactionMove,
        BattleEvent e,
        FilterRule[] filters,
        out List<BattleCombatant> resolvedTargets)
    {
        resolvedTargets = new();
        if (!Matches(checker, e)) return false;

        resolvedTargets = ApplyTargetExclusions(ResolveTargets(checker, reactionMove, e, targetSelector, filters));
        return resolvedTargets != null && resolvedTargets.Count > 0;
    }

    public bool TryResolveTargets(BattleEvent e, out List<BattleCombatant> resolvedTargets)
    {
        return TryResolveTargets(null, e, out resolvedTargets);
    }

    // ----------------------------
    // Exclusions
    // ----------------------------

    private List<BattleCombatant> ApplyTargetExclusions(List<BattleCombatant> input)
    {
        if (input == null || input.Count == 0) return new();
        if (!excludeDefeatedTargets) return input;

        List<BattleCombatant> list = new(input.Count);
        for (int i = 0; i < input.Count; i++)
        {
            var t = input[i];
            if (t == null) continue;

            // If hero/health is missing, treat as invalid target.
            // If your game wants otherwise, flip this default.
            if (t.hero == null || t.hero.health == null) continue;

            // Exclude defeated
            if (t.hero.health.IsDead) continue;

            list.Add(t);
        }
        return list;
    }

    // ----------------------------
    // Target resolvers (unchanged)
    // ----------------------------

    public static List<BattleCombatant> ResolveTargets(BattleCombatant checker, BattleEvent e, Enum_BattleCondition_Targets selector)
    {
        if (e == null) return new();

        return selector switch
        {
            Enum_BattleCondition_Targets.owner =>
                e.owner != null ? new List<BattleCombatant> { e.owner } : new(),

            Enum_BattleCondition_Targets.source =>
                e.source != null ? new List<BattleCombatant> { e.source } : new(),

            Enum_BattleCondition_Targets.target =>
                (e.targets != null && e.targets.Count > 0) ? e.targets : new(),

            Enum_BattleCondition_Targets.self =>
                checker != null ? new List<BattleCombatant> { checker } : new(),

            Enum_BattleCondition_Targets.all =>
                new(BattleCombatant.AllCombatants),

            Enum_BattleCondition_Targets.allies =>
                e.owner != null ? BattleCombatant.AllAlliesOf(e.owner) : new(),

            Enum_BattleCondition_Targets.enemies =>
                e.owner != null ? BattleCombatant.AllEnemiesOf(e.owner) : new(),

            Enum_BattleCondition_Targets.prefered =>
                new(),

            _ => new(),
        };
    }

    public static List<BattleCombatant> ResolveTargets(
        BattleCombatant checker,
        BattleMove reactionMove,
        BattleEvent e,
        Enum_BattleCondition_Targets selector,
        FilterRule[] filters)
    {
        if (e == null) return new();

        return selector switch
        {
            Enum_BattleCondition_Targets.owner =>
                e.owner != null ? new List<BattleCombatant> { e.owner } : new(),

            Enum_BattleCondition_Targets.source =>
                e.source != null ? new List<BattleCombatant> { e.source } : new(),

            Enum_BattleCondition_Targets.target =>
                (e.targets != null && e.targets.Count > 0) ? e.targets : new(),

            Enum_BattleCondition_Targets.self =>
                checker != null ? new List<BattleCombatant> { checker } : new(),

            Enum_BattleCondition_Targets.all =>
                new(BattleCombatant.AllCombatants),

            Enum_BattleCondition_Targets.prefered =>
                (checker != null && reactionMove != null)
                    ? reactionMove.ResolveTargets(checker, filters)
                    : new(),

            _ => new(),
        };
    }
}
