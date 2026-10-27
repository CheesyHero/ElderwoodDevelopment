using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class BattlePlan
{ 
    public List<MovePriorityEntry> skills = new();  
    [System.NonSerialized] protected List<long> _respondedIds = new(64);
    public void Initialize(Hero hero)
    {
        if (hero == null) return;

        // If empty, build fresh.
        if (skills == null || skills.Count == 0)
            GenerateSkillFresh(hero, false);

        // Always ensure plan contains all known moves (including reactions).
        AddMissingTech(hero);

        // Optional: if any planned move is no longer known, regenerate.
        ValidateMoves(hero);

        SortByPriority();
    } 
    public void ChangeMove(BattleMove move, int index)
    {
        if (index < 0) return;

        if (index < skills.Count)
        {
            skills[index].desiredMove = move;
            skills[index].filters = new();
        }
        else skills.Add(new(move, false));

        SortByPriority();
    }
    public void GenerateSkillFresh(Hero load, bool recommendedSkills)
    { 
        skills = new();
        if (load == null) return; 

        foreach(var item in load.KnownSkills()) 
            skills.Add(new(item, recommendedSkills));

        //Debug.Log($"{load} has generated a new list of skills. Preparing to sort...");

        SortByPriority();
    }
    public void ReloadSkills(Hero hero)
    {
        if (hero == null) return;

        skills ??= new List<MovePriorityEntry>();

        if (skills.Count == 0)
            GenerateSkillFresh(hero, false);
        else
        {
            // Key change: do NOT return early; keep plan in sync.
            AddMissingTech(hero);
            SortByPriority();
        }
    }
    public void ValidateMoves(Hero hero)
    {
        if (hero == null) return;

        List<BattleMove> known = hero.KnownSkills();

        AddMissingTech(hero);

        for (int i = 0; i < skills.Count; i++)
        {
            BattleMove planned = skills[i].desiredMove;
            if (planned == null || !known.Contains(planned))
            {
                GenerateSkillFresh(hero, false);
                return;
            }
        } 
    }
    public void AddMissingTech(Hero hero)
    {
        List<BattleMove> known = hero.KnownSkills();
         
        foreach (var move in known)
        { 
            bool existsInPlan = hero.plan.skills
                .Any(s => s.desiredMove == move);

            if (!existsInPlan) 
                hero.plan.skills.Add(new(move, false)); 
        } 
    } 

    private static int RankMoveType(MoveType t)
    {
        return t switch
        {
            MoveType.special => 0,
            MoveType.action => 1,
            MoveType.reaction => 2,
            _ => 99,
        };
    } 
    public void SortByPriority()
    {
        if (skills == null || skills.Count <= 1) return;

        skills.Sort((a, b) =>
        {
            if (a == null && b == null) return 0;
            if (a == null) return 1;
            if (b == null) return -1;

            if (a.desiredMove == null && b.desiredMove == null) return 0;
            if (a.desiredMove == null) return 1;
            if (b.desiredMove == null) return -1;

            BattleMove ma = a.desiredMove;
            BattleMove mb = b.desiredMove;

            // 1) Type: Special -> Action -> Reaction
            int typeCompare = RankMoveType(ma.moveType).CompareTo(RankMoveType(mb.moveType));
            if (typeCompare != 0) return typeCompare;

            // 2) Slot priority: lower first
            int priorityCompare = a.priority.CompareTo(b.priority);
            if (priorityCompare != 0) return priorityCompare;

            // 3) Cost: lower first
            int costCompare = ma.cost.CompareTo(mb.cost);
            if (costCompare != 0) return costCompare;

            // 4) Deterministic tie-break
            return string.Compare(ma.skillName, mb.skillName, System.StringComparison.Ordinal);
        });
    }

     
    public void ResetResponseLedger()
    {
        _respondedIds ??= new List<long>(64);
        _respondedIds.Clear();
    }

    public bool HasRespondedTo(long eventId, BattleCombatant checker)
    { 
        if (eventId == 0 || checker == null) return false;
        long key = (eventId << 32) ^ (uint)checker.GetInstanceID();
        return _respondedIds.Contains(key);
    }

    public void MarkResponded(long eventId, BattleCombatant checker)
    {
        if (eventId == 0 || checker == null) return;
        long key = (eventId << 32) ^ (uint)checker.GetInstanceID();
        if (!_respondedIds.Contains(key))
            _respondedIds.Add(key);
    }

    // Finds a reaction and returns the desired move to use
    public BattleEvent Compare(BattleCombatant checker, BattleEvent action)
    {
        if (action == null || checker == null) return null;
        if (HasRespondedTo(action.instanceId, checker)) return null;

        foreach (var move in skills)
        {
            if (move == null || move.desiredMove == null) continue;
            if (move.desiredMove.moveType != MoveType.reaction) continue;

            BattleEvent response = move.Confirm(checker, action);
            if (response != null)
            {
                //Debug.LogWarning($"{checker} has a response for " +
                //    $"(<color=orange>{action.instanceId}</color> | <color=white>{action.eventType}</color>) with : " +
                //    $"(<color=orange>{response.instanceId}</color> | <color=white>{response.eventType}</color>) with : ");
                MarkResponded(action.instanceId, checker);
                return response;
            }
        }

        return null;
    }
    public BattleEvent GetActionEvent(BattleCombatant user)
    {
        if (user == null || skills.Count == 0) return null;
         
        foreach (var entry in skills)
        { 
            if (entry.desiredMove == null || !entry.desiredMove.CanCast(user)) 
                continue; 
             
            BattleEvent ev = entry.BuildActionEvent(user);
            if (ev != null && ev.targets != null && ev.targets.Count > 0)
                return ev;
        }

        return null;
    }
    public BattlePlan Copy()
    {
        BattlePlan copy = new();

        // Deep copy skills list (but share BattleMove references)
        copy.skills = new List<MovePriorityEntry>(skills.Count);
        foreach (var entry in skills)
        {
            if (entry == null) continue;

            MovePriorityEntry e = new(entry.desiredMove, false)
            {
                priority = entry.priority,
                filters = entry.filters != null
                    ? new List<FilterRule>(entry.filters)
                    : new List<FilterRule>()
            };

            copy.skills.Add(e);
        }

        // Fresh response ledger (never share this)
        copy._respondedIds = new List<long>(64);

        return copy;
    }
}



