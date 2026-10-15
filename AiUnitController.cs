using UnityEngine;
using System.Collections.Generic; 

public enum Enum_AI_Contoller_Modes
{
    idle, // stays in one place
    march, // always advances to the highest priority
    aggressive, // always moves to the closest known opponent
    defender, // searches for the closest base and stays there
    evasive, // moves away from the nearest threat, attempts to capture the closes base
    random,
}
public class AiUnitController : HexWalker
{
    [Header("AI Tools")]
    public Enum_AI_Contoller_Modes controllerMode;  

    public void AI_PushMove()
    {
        if (BattleInProgress) return;
        if (IsWalking) return;
        if (moves <= 0) return; 

        switch (controllerMode)
        {
            case Enum_AI_Contoller_Modes.idle:
                HandleIdle();
                break;

            case Enum_AI_Contoller_Modes.march:
                HandleMarch();
                break;

            case Enum_AI_Contoller_Modes.aggressive:
                HandleAggressive();
                break;

            case Enum_AI_Contoller_Modes.defender:
                HandleDefender();
                break;

            case Enum_AI_Contoller_Modes.evasive:
                HandleEvasive();
                break;
        }
    }
    private void HandleIdle()
    {
        // Do nothing this turn.
        moves = 0;
    }
    private void HandleMarch()
    {
        HexTile target = FindClosestPriorityTile();

        if (target != null)
            TryMoveToward(target);

        else moves = 0;

    }
    private void HandleAggressive()
    {
        HexWalker enemy = FindClosestEnemy();

        if (enemy != null)
            TryMoveToward(enemy.CurrentTile);

        else HandleMarch();

    }
    private void HandleDefender()
    {
        TeamBase nearestBase = FindClosestBase();

        if (nearestBase == null)
        {
            moves = 0;
            return;
        }

        // If already at the base: hold position
        if (nearestBase == CurrentTile)
        {
            moves = 0;
            return;
        }

        TryMoveToward(nearestBase);
    }
    private void HandleEvasive()
    {
        HexWalker threat = FindClosestEnemy();

        if (threat != null)
        {
            // Move away from threat
            HexTile safest = FindTileFurthestFrom(threat);

            if (safest != null)
            {
                TryMoveToward(safest);
                return;
            }
        }

        // No threat -> go capture a base
        TeamBase baseToCapture = FindClosestBase();

        if (baseToCapture == null)
        {
            moves = 0;
            return;
        }

        TryMoveToward(baseToCapture);
    } 

    // Movement wrapper
    private void TryMoveToward(HexTile goal)
    {
        if (IsWalking) return;

        if (goal == null || goal == CurrentTile)
        { moves = 0; return; }

        // 1) Prefer cost-1 tiles first
        HexTile step = PickBestAdjacentToward(goal.transform.position, preferCost1: true);

        // 2) If no cost-1 option (all were allies or none exist), allow cost-2
        if (step == null)
            step = PickBestAdjacentToward(goal.transform.position, preferCost1: false);

        if (step == null)
        { moves = 0; return; }

        int stepCost = GetMoveCostForTile(step);

        // If our only viable option is a cost-2 tile and we can't afford it, cancel turn.
        if (stepCost == 2 && moves <= 1)
        { moves = 0; return; }

        // Occupancy resolution:
        // Enemy -> battle
        // Ally  -> should not happen (filtered), but handle safely by avoiding
        // Empty -> walk
        if (IsEnemyOnTile(step))
        {
            Battle(step.CurrentUnit);
            // moves = 0;
            return;
        }

        if (IsAllyOnTile(step))
        {
            // Safety: try any other available step (cost-1 first, then cost-2)
            HexTile alt = PickBestAdjacentToward(goal.transform.position, preferCost1: true);
            if (alt == null) alt = PickBestAdjacentToward(goal.transform.position, preferCost1: false);

            if (alt == null)
            { moves = 0; return; }

            int altCost = GetMoveCostForTile(alt);
            if (altCost == 2 && moves <= 1)
            { moves = 0; return; }

            if (IsEnemyOnTile(alt)) { Battle(alt.CurrentUnit); return; }
            WalkTo(alt);
            return;
        }

        // Empty tile
        WalkTo(step);
    }
    public HexWalker FindClosestEnemy()
    {
        // Goes through and removes all important points we already own
        List<HexWalker> temporaryList = new(activeWalkers); // DO NOT REMOVE WALKERS

        foreach (var item in temporaryList.ToArray())
            if (item.Parent.IsAlive() && item.EnemyVisible())
                temporaryList.Remove(item);

        return (HexWalker)ReturnClosestToPoint(temporaryList.ToArray(), transform.position);
    }    
    public HexTile FindClosestPriorityTile()
    {
        // Goes through and removes all important points we already own
        List<TeamBase> allbases = new(HexCrawlManager.Bases);
        foreach (var item in allbases.ToArray())
            if (item.owner == Team)
                allbases.Remove(item);

        return (HexTile)ReturnClosestToPoint(allbases.ToArray(), transform.position);
    }
    public TeamBase FindClosestBase()
    {
        if (CurrentTile is TeamBase b && b.owner != null && b.owner.CompareAllied(Team))
            return b;

        List<TeamBase> bases = new(TeamBase.bases);
        foreach (var item in bases.ToArray())
        {
            if (item.CurrentUnit && item.CurrentUnit.Team == Team)
                bases.Remove(item);
        }

        return (TeamBase)ReturnClosestToPoint(bases.ToArray(), transform.position);
    }
    public HexTile FindTileFurthestFrom(HexWalker threat)
    { 
        return (HexTile)ReturnFurthestFromPoint(CurrentTile.adjacents.ToArray(), threat.transform.position);
    }
    private HexTile GetNearestAvailableInDirection(HexTile current, Vector3 dirWorldPos, int attempts = 6)
    {
        if (current == null) return null;

        return current.GetClosestAdjacentInDirection(dirWorldPos);
    }
    private int GetMoveCostForTile(HexTile tile)
    {
        if (!tile) return 999;

        var traits = Leader().heroClass.leaderTraits;
        int penalty = traits.ContainsUnfavoredType(CurrentTile) ? 1 : 0;
        if (traits.ContainsNoWalkType(tile)) return -1;
        else return (traits.ContainsUnfavoredType(tile) ? 2 : 1) + penalty;
    }

    private bool IsAllyOnTile(HexTile tile)
    {
        if (!tile || tile.CurrentUnit == null) return false;
        return tile.CurrentUnit.Team.CompareAllied(Team);
    }

    private bool IsEnemyOnTile(HexTile tile)
    {
        if (!tile || tile.CurrentUnit == null) return false;
        return !tile.CurrentUnit.Team.CompareAllied(Team);
    }

    // Picks the best adjacent "step" toward point, respecting terrain costs and occupancy rules.
    // preferCost1: if true, only considers cost=1 candidates (unless none exist, then returns null).
    private HexTile PickBestAdjacentToward(Vector3 point, bool preferCost1)
    {
        HexTile best = null;
        float bestScore = float.PositiveInfinity;

        foreach (var tile in CurrentTile.adjacents)
        {
            if (!tile) continue;

            int moveCost = GetMoveCostForTile(tile);
            if (preferCost1 && moveCost != 1)
                continue;

            // Avoid allied occupied tiles in selection
            if (IsAllyOnTile(tile))
                continue;

            float score = ModScore(tile, Vector3.Distance(tile.transform.position, point)); 

            if (score < bestScore)
            {
                bestScore = score;
                best = tile;
            }
        }

        return best;
    }
    public float ModScore(HexTile tile, float score)
    {
        // road preference (still fine)
        if (tile.type == Enum_HexTile_Type.road)
            score -= 1f;

        // avoid ping-pong
        if (tile == LastTile)
            score += 1f;

        // If enemy is on tile, bias toward engagement (but still directional)
        if (IsEnemyOnTile(tile))
            score -= 3.0f;

        // If cost 2 (only possible when preferCost1 == false), de-prioritize
        int cost = GetMoveCostForTile(tile);
        if (cost > 1) score += cost;

        return score;
    }

    public void GenerateMode()
    {
        if (CurrentTile is TeamBase)
            controllerMode = Enum_AI_Contoller_Modes.defender;

        else controllerMode = GetRandomMode();
    }
    public Enum_AI_Contoller_Modes GetRandomMode()
    {
        if (controllerMode != Enum_AI_Contoller_Modes.random)
            return controllerMode;

        return (Random.Range(0, 99) % 3) switch
        {
            1 => Enum_AI_Contoller_Modes.aggressive, // aggro units will prioritize enemy units before bases
            2 => Enum_AI_Contoller_Modes.defender, // defenders will find the nearest non-allied base and defend it
            //0                                    // if the only base unallied in the player base, will attempt to take it
            _ => Enum_AI_Contoller_Modes.march,    // Will move towards the player base. Will attack adjacent enemies.
        };
    }
}
