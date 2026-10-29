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
        List<BattleCombatant> targets =
            desiredMove.ResolveTargets(actor, filters?.ToArray()); 

        BattleEvent result = new BattleEvent()
        {
            instanceId = 0,
            eventType = Enum_BattleEvent_Type.onTurnStart,
            move = desiredMove,
            owner = actor,
            source = actor,
            targets = targets,
            value = desiredMove.cost,
        }; 

        return result;
    }

    public void ChangePlan(FilterRule next)
    {
        if (next == null) return;

        if (filters.Contains(next))
        {
            filters.Remove(next);
            return;
        }

        // If the same rule type exists, replace it.
        for (int i = 0; i < filters.Count; i++)
        {
            if (filters[i] == null || filters[i].type != next.type)
                continue;

            filters[i] = next;

            // Maintain Picker-first ordering.
            filters.Sort((a, b) => a.mode.CompareTo(b.mode));
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

        filters.Sort((a, b) => a.mode.CompareTo(b.mode));
    }
}
