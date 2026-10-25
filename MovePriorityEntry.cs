using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class MovePriorityEntry
{
    [Range(0, 8)] public int priority = 0;
    public BattleMove desiredMove;
    public List<FilterRule> filters = new();

    public MovePriorityEntry(BattleMove move, bool recommended)
    {
        desiredMove = move;

        if (!recommended && filters.Count == 0)
            filters = new(move.defaultRules);

        else filters = new(move.recommendedRules);
    }

    public void TickPriority(int direction)
    {
        priority += direction;

        if (priority > 8)
            priority = 0;
        else if (priority < 0)
            priority = 8;
    }

    public BattleEvent Confirm(BattleCombatant checker, BattleEvent action)
    {
        if (desiredMove == null) return null;
        return desiredMove.GetReaction(checker, action, filters.ToArray());
    }

    public BattleEvent BuildActionEvent(BattleCombatant actor)
    {
        if (desiredMove == null || actor == null) return null;

        List<BattleCombatant> targets = desiredMove.ResolveTargets(actor, filters?.ToArray());
        if (targets == null || targets.Count == 0) return null;

        return new BattleEvent()
        {
            instanceId = 0,
            eventType = Enum_BattleEvent_Type.onTurnStart,
            move = desiredMove,
            owner = actor,
            source = actor,
            targets = targets,
            value = desiredMove.cost,
        };
    }

    public void ChangePlan(FilterRule next)
    {
        if (next == null) return;

        if (filters.Contains(next))
        {
            filters.Remove(next);
            return;
        }

        // Only one Picker can exist at a time.
        if (next.mode == E_MoveFilterMode.picker)
        {
            filters.RemoveAll(rule =>
                rule != null && rule.mode == E_MoveFilterMode.picker);
        }

        // Maintain the maximum of 2 rules.
        if (filters.Count >= 2)
            filters.RemoveAt(0);

        // Pickers always occupy the first position.
        if (next.mode == E_MoveFilterMode.picker)
            filters.Insert(0, next);
        else
            filters.Add(next);

        // Ensure Picker is always first, even after cycling.
        filters.Sort((a, b) => a.mode.CompareTo(b.mode));
    }
}
