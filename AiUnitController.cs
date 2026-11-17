using System.Collections.Generic;
using UnityEngine;

public enum Enum_AI_Contoller_Modes
{
    idle,       // Stay in place.
    march,      // Advance toward a base the team doesn't own.
    aggressive, // Pursue the nearest visible enemy, otherwise march.
    defender,   // Hold an allied base or move toward a base to defend.
    evasive,    // Move away from nearby threats, otherwise seek a base.
    random,
}

public class AiUnitController : HexWalker
{
    [Header("AI Tools")]
    public Enum_AI_Contoller_Modes controllerMode;

    [Header("Pathfinding")]
    [Min(0f)]
    [Tooltip("Extra A* preference cost for entering an unfavored terrain tile. " +
             "Does NOT consume additional movement points. Raise to avoid water more strongly.")]
    public float unfavoredPathPenalty = 6f;

    [Header("Wilderness Targeting")]
    [Range(0f, 1f)]
    [Tooltip("Chance to attack an adjacent Wilderness unit if no rival-army unit is adjacent. " +
             "Wilderness is never pursued over longer distances.")]
    public float wildernessEngagementChance = 0.25f;

    [Header("Evasive Retreat")]
    [Min(0f)] public float retreatBaseWeight = 1.5f;
    [Min(0f)] public float retreatEnemyWeight = 2.5f;
    [Min(0f)] public float retreatThreatRadius = 5f;
    [Min(0f)] public float retreatAdjacentEnemyPenalty = 3f;
    [Min(0f)] public float retreatUnfavoredPenalty = 2f;

    private Enum_AI_Contoller_Modes lastMode;
    private bool forcedEvasive;

    // A* only chooses the NEXT step. HexWalker's existing Move/Battle
    // logic remains responsible for animations, costs, and combat.
    public void AI_PushMove()
    {
        if (BattleInProgress || IsWalking || moves <= 0 || !CurrentTile)
            return;

        // Update the retreat state BEFORE any automatic enemy engagement.
        if (forcedEvasive)
            CheckIfShouldExitEvasive();
        else
            CheckIfShouldBeEvasive();

        // Marching/defending/aggressive units must not walk away from an
        // immediately attackable rival army just to reach another objective.
        // Evasive and idle modes keep their intentional non-aggressive behavior.
        if (controllerMode == Enum_AI_Contoller_Modes.march ||
            controllerMode == Enum_AI_Contoller_Modes.aggressive ||
            controllerMode == Enum_AI_Contoller_Modes.defender)
        {
            HexWalker adjacentRival = FindAdjacentEnemy(includeWilderness: false);
            if (adjacentRival)
            {
                Battle(adjacentRival);
                return;
            }
        }

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
            case Enum_AI_Contoller_Modes.random:
                // Treat an uninitialized Random mode as a request to pick one.
                controllerMode = GetRandomMode();
                AI_PushMove();
                break;
        }
    }

    private void HandleIdle() => moves = 0;

    private void HandleMarch()
    {
        HexTile goal = FindClosestPriorityTile();
        if (goal) TryMoveToward(goal);
        else moves = 0;
    }

    private void HandleAggressive()
    {
        // Opportunistic Wilderness combat only happens at melee/hex adjacency.
        // Never abandon an adjacent rival to chase a neutral creature.
        HexWalker adjacentWilderness = FindAdjacentEnemy(includeWilderness: true);
        if (adjacentWilderness && Random.value < wildernessEngagementChance)
        {
            Battle(adjacentWilderness);
            return;
        }

        // Strategic enemy targeting excludes Wilderness entirely.
        HexWalker rival = FindClosestEnemy();
        if (rival && rival.CurrentTile)
            TryMoveToward(rival.CurrentTile);
        else
            HandleMarch();
    }

    private void HandleDefender()
    {
        TeamBase goal = FindClosestBase();
        if (!goal || goal == CurrentTile)
        {
            moves = 0;
            return;
        }

        TryMoveToward(goal);
    }

    private void HandleEvasive()
    {
        TeamBase alliedBase = FindNearestRetreatBase();

        // Already resting on an allied base: don't step off it to flee.
        if (alliedBase && CurrentTile == alliedBase)
        {
            moves = 0;
            return;
        }

        // Ask a defender on an adjacent allied base to vacate it.
        SetNearbyDefenderToMarchIfSelfIsEvasive();

        Vector3 enemyCenter;
        bool hasEnemies = TryGetEnemyCenter(out enemyCenter);
        HexTile best = null;
        float bestScore = float.NegativeInfinity;

        // Evaluate ALL moves that can actually be made this turn.
        // Do not require a complete A* route to a potentially occupied base.
        foreach (HexTile tile in CurrentTile.adjacents)
        {
            if (!tile || IsOccupied(tile)) continue;

            int cost = GetMoveCostForTile(CurrentTile, tile);
            if (cost < 0 || cost > moves) continue;

            float score = 0f;

            if (alliedBase)
            {
                // Reward making progress toward a place to recover.
                float before = Vector3.Distance(transform.position, alliedBase.transform.position);
                float after = Vector3.Distance(tile.transform.position, alliedBase.transform.position);
                score += (before - after) * retreatBaseWeight;
                if (tile == alliedBase) score += 100f;
            }

            if (hasEnemies)
            {
                // Reward putting distance between us and the enemy group's center.
                float before = Vector3.Distance(transform.position, enemyCenter);
                float after = Vector3.Distance(tile.transform.position, enemyCenter);
                score += (after - before) * retreatEnemyWeight;
            }

            // Avoid stepping directly next to an enemy whenever possible.
            foreach (HexTile neighbor in tile.adjacents)
            {
                if (neighbor && neighbor.CurrentUnit && IsValidRetreatThreat(neighbor.CurrentUnit))
                    score -= retreatAdjacentEnemyPenalty;
            }

            if (IsUnfavored(tile)) score -= retreatUnfavoredPenalty;
            if (score > bestScore)
            {
                bestScore = score;
                best = tile;
            }
        }

        if (best)
        {
            // Retreatment never attacks: selected tile is guaranteed empty.
            int cost = GetMoveCostForTile(CurrentTile, best);
            best.UpdateCost(cost);
            WalkTo(best);
            return;
        }

        // No legal neighboring tile can be reached with remaining moves.
        moves = 0;
    }

    // Finds the nearest allied base, including an occupied base that a
    // friendly Defender can eventually vacate. Prefer currently free bases.
    private TeamBase FindNearestRetreatBase()
    {
        TeamBase closestOpen = null;
        TeamBase closestDefended = null;
        float openDistance = float.PositiveInfinity;
        float defendedDistance = float.PositiveInfinity;

        foreach (TeamBase teamBase in TeamBase.bases)
        {
            if (!teamBase || teamBase.owner == null ||
                !teamBase.owner.CompareAllied(Team)) continue;

            HexWalker occupant = teamBase.CurrentUnit;
            if (occupant && occupant != this &&
                (occupant.Army != Army ||
                 occupant is not AiUnitController defender ||
                 defender.controllerMode != Enum_AI_Contoller_Modes.defender))
                continue;

            float distance = Vector3.Distance(transform.position, teamBase.transform.position);
            if (!occupant || occupant == this)
            {
                if (distance < openDistance)
                {
                    closestOpen = teamBase;
                    openDistance = distance;
                }
            }
            else if (distance < defendedDistance)
            {
                closestDefended = teamBase;
                defendedDistance = distance;
            }
        }

        // A nearby occupied Defender base may be worth clearing instead of
        // retreating toward a much more distant unoccupied base.
        if (closestDefended && defendedDistance < openDistance)
            return closestDefended;
        return closestOpen ? closestOpen : closestDefended;
    }

    private bool IsValidRetreatThreat(HexWalker walker)
    {
        return walker && walker != this && walker.CurrentTile &&
               walker.Army != null && walker.Parent != null &&
               walker.Parent.IsAlive() && !walker.Team.CompareAllied(Team) &&
               walker.EnemyVisible();
    }

    private bool TryGetEnemyCenter(out Vector3 center)
    {
        center = Vector3.zero;
        int count = 0;

        foreach (HexWalker walker in activeWalkers)
        {
            if (!IsValidRetreatThreat(walker)) continue;
            if (Vector3.Distance(transform.position, walker.transform.position) > retreatThreatRadius)
                continue;
            center += walker.transform.position;
            count++;
        }

        if (count == 0) return false;
        center /= count;
        return true;
    }

    public void SetNearbyDefenderToMarchIfSelfIsEvasive()
    {
        if (controllerMode != Enum_AI_Contoller_Modes.evasive || !CurrentTile)
            return;

        foreach (HexTile tile in CurrentTile.adjacents)
        {
            if (tile is not TeamBase teamBase || teamBase.owner == null ||
                !teamBase.owner.CompareAllied(Team)) continue;

            if (teamBase.CurrentUnit is not AiUnitController ally ||
                ally == this || ally.Army != Army ||
                ally.controllerMode != Enum_AI_Contoller_Modes.defender)
                continue;

            ally.controllerMode = Enum_AI_Contoller_Modes.march;
            return;
        }
    }

    public void CheckIfShouldBeEvasive()
    {
        if (forcedEvasive || controllerMode == Enum_AI_Contoller_Modes.evasive ||
            Parent == null || Parent.Leader() == null) return;

        if (Parent.GetTotalHealthPercent() < 0.5f || Parent.Leader().health.IsDead)
        {
            lastMode = controllerMode;
            controllerMode = Enum_AI_Contoller_Modes.evasive;
            forcedEvasive = true;
        }
    }

    public void CheckIfShouldExitEvasive()
    {
        if (!forcedEvasive || Parent == null || Parent.Leader() == null) return;

        if (Parent.GetTotalHealthPercent() > 0.5f && Parent.Leader().health.IsAlive)
        {
            controllerMode = lastMode;
            forcedEvasive = false;
        }
    }

    // -------- Destination selection (mostly reused) --------

    // Returns only rival-army enemies. Wilderness is deliberately excluded.
    // Used by aggressive and evasive behaviors so they prioritize real opponents.
    public HexWalker FindClosestEnemy()
    {
        List<HexWalker> candidates = new();

        foreach (HexWalker walker in activeWalkers)
        {
            if (!IsValidEnemy(walker) || IsWilderness(walker))
                continue;

            candidates.Add(walker);
        }

        return (HexWalker)ReturnClosestToPoint(candidates.ToArray(), transform.position);
    }

    // Nearby Wilderness is a possible incidental fight for Aggressive AI only,
    // never a pursuit target.
    private HexWalker FindAdjacentEnemy(bool includeWilderness)
    {
        if (!CurrentTile || CurrentTile.adjacents == null)
            return null;

        foreach (HexTile tile in CurrentTile.adjacents)
        {
            if (!tile) continue;

            HexWalker walker = tile.CurrentUnit;
            if (!IsValidEnemy(walker)) continue;
            if (IsWilderness(walker) != includeWilderness) continue;

            // Battle() uses the leader's movement cost independently;
            // retaining the same mobility gate as AI_PushMove().
            return walker;
        }

        return null;
    }

    private bool IsWilderness(HexWalker walker)
    {
        return walker && walker.Army != null &&
               walker.Army == EnemyArmyManager.Wilderness;
    }

    private bool IsValidEnemy(HexWalker walker)
    {
        if (!walker || walker == this || !walker.CurrentTile ||
            walker.Army == null || !walker.Parent.IsAlive())
            return false;

        if (walker.Team.CompareAllied(Team))
            return false;

        // Preserve the previous visibility rule.
        return walker.EnemyVisible();
    }

    public HexTile FindClosestPriorityTile()
    {
        // March only targets neutral/unclaimed bases or bases owned by a rival in AllArmies.
        // Wilderness-controlled bases are not strategic objectives.
        List<TeamBase> candidates = new(HexCrawlManager.Bases);
        candidates.RemoveAll(b => !IsStrategicBaseTarget(b));
        return (HexTile)ReturnClosestToPoint(candidates.ToArray(), transform.position);
    }

    public TeamBase FindClosestBase()
    {
        // Preserve the existing defender behavior: hold an allied base.
        if (CurrentTile is TeamBase currentBase &&
            currentBase.owner != null && currentBase.owner.CompareAllied(Team))
            return currentBase;

        List<TeamBase> candidates = new(TeamBase.bases);
        candidates.RemoveAll(b => !IsStrategicBaseTarget(b));

        return (TeamBase)ReturnClosestToPoint(candidates.ToArray(), transform.position);
    }

    // A strategic base is either unclaimed or owned by an army registered
    // in AllArmies. Wilderness and unknown independent factions are excluded.
    // An occupied base with a Wilderness guard is also excluded: only
    // Aggressive AI should ever initiate a fight against Wilderness.
    private bool IsStrategicBaseTarget(TeamBase targetBase)
    {
        if (!targetBase) return false;

        // Never prioritize a base already held by this team or its allies.
        if (targetBase.owner != null && targetBase.owner.CompareAllied(Team))
            return false;

        if (targetBase.CurrentUnit && targetBase.CurrentUnit != this)
        {
            if (targetBase.CurrentUnit.Team.CompareAllied(Team)) return false;
            if (IsWilderness(targetBase.CurrentUnit)) return false;
        }

        // These are the unclaimed defaults used by HexTile and TeamBase.
        Team owner = targetBase.owner;
        if (owner == null || string.IsNullOrWhiteSpace(owner.teamName) ||
            owner.teamName == "Neutral" || owner.teamName == "No Allegiance")
            return true;

        // Never select a base owned by Wilderness, even if it is unoccupied.
        if (EnemyArmyManager.Wilderness &&
            owner.CompareAllied(EnemyArmyManager.Wilderness.team))
            return false;

        foreach (Army army in EnemyArmyManager.AllArmies)
        {
            if (army && army.team != null && owner.CompareAllied(army.team))
                return true;
        }

        return false;
    }

    public HexTile FindTileFurthestFrom(HexWalker threat)
    {
        if (!threat || !CurrentTile) return null;

        HexTile best = null;
        float bestDistance = float.NegativeInfinity;

        foreach (HexTile tile in CurrentTile.adjacents)
        {
            if (!tile || GetMoveCostForTile(CurrentTile, tile) > moves ||
                GetMoveCostForTile(CurrentTile, tile) < 0 || IsOccupied(tile))
                continue;

            float distance = Vector3.Distance(tile.transform.position, threat.transform.position);
            if (distance > bestDistance)
            {
                bestDistance = distance;
                best = tile;
            }
        }

        return best;
    }

    // -------- A* movement (new) --------

    // Returns true if a step or attack was started. The path is recalculated
    // on the next AI move, so changes in occupancy and terrain are respected.
    private bool TryMoveToward(HexTile goal)
    {
        if (IsWalking || !CurrentTile || !goal || goal == CurrentTile)
        {
            moves = 0;
            return false;
        }

        List<HexTile> path = FindPath(CurrentTile, goal);
        if (path == null || path.Count < 2)
        {
            moves = 0;
            return false;
        }

        HexTile step = path[1];
        int cost = GetMoveCostForTile(CurrentTile, step);

        if (cost < 0 || cost > moves)
        {
            // Don't try another adjacent tile: it may be cheaper this turn
            // but worse for the complete route. Finish this unit's turn.
            moves = 0;
            return false;
        }

        if (IsEnemyOnTile(step))
        {
            // Wilderness is a valid combat target ONLY for Aggressive mode.
            if (IsWilderness(step.CurrentUnit) &&
                controllerMode != Enum_AI_Contoller_Modes.aggressive)
            {
                moves = 0;
                return false;
            }

            Battle(step.CurrentUnit);
            return true;
        }

        if (IsOccupied(step))
        {
            moves = 0;
            return false;
        }

        // WalkTo() reads HexTile.cost (which is normally set by the UI).
        // Supply the calculated cost so AI movement is charged correctly.
        step.UpdateCost(cost);
        WalkTo(step);
        return true;
    }

    private List<HexTile> FindPath(HexTile start, HexTile goal)
    {
        if (!start || !goal) return null;
        if (start == goal) return new List<HexTile> { start };
        if (!CanTraverse(goal, goal)) return null;

        List<HexTile> open = new() { start };
        HashSet<HexTile> closed = new();
        Dictionary<HexTile, HexTile> previous = new();
        Dictionary<HexTile, float> gScore = new() { [start] = 0f };

        // World-distance heuristic scaled by the longest adjacent tile edge.
        // Each traversable edge costs at least 1, making this admissible.
        float longestEdge = GetLongestAdjacentDistance();

        while (open.Count > 0)
        {
            HexTile current = GetLowestFScore(open, gScore, goal, longestEdge);
            if (current == goal)
                return ReconstructPath(previous, current);

            open.Remove(current);
            closed.Add(current);

            foreach (HexTile next in current.adjacents)
            {
                if (!CanTraverse(next, goal) || closed.Contains(next))
                    continue;

                int stepCost = GetMoveCostForTile(current, next);
                if (stepCost < 0) continue;

                // Actual movement cost + preference penalty for unfavored terrain.
                // The preference penalty ONLY influences the selected route;
                // it is never charged to HexWalker's movement points.
                float preferencePenalty = IsUnfavored(next) ? unfavoredPathPenalty : 0f;
                float tentative = gScore[current] + stepCost + preferencePenalty;
                if (gScore.TryGetValue(next, out float known) && tentative >= known)
                    continue;

                previous[next] = current;
                gScore[next] = tentative;

                if (!open.Contains(next)) open.Add(next);
            }
        }

        return null; // No accessible path.
    }

    private HexTile GetLowestFScore(
        List<HexTile> open,
        Dictionary<HexTile, float> gScore,
        HexTile goal,
        float longestEdge)
    {
        HexTile best = open[0];
        float bestScore = float.PositiveInfinity;

        foreach (HexTile tile in open)
        {
            float score = gScore[tile] + GetHeuristic(tile, goal, longestEdge);
            if (score < bestScore)
            {
                bestScore = score;
                best = tile;
            }
        }

        return best;
    }

    private float GetHeuristic(HexTile from, HexTile goal, float longestEdge)
    {
        // Lower bound on required hex steps. No road bonus is subtracted,
        // because negative edge costs would break A*'s optimality guarantee.
        return Vector3.Distance(from.transform.position, goal.transform.position) /
               Mathf.Max(0.001f, longestEdge);
    }

    private float GetLongestAdjacentDistance()
    {
        float longest = 0f;
        foreach (HexTile tile in HexTile.AllTiles)
        {
            if (!tile) continue;
            foreach (HexTile neighbor in tile.adjacents)
            {
                if (!neighbor) continue;
                longest = Mathf.Max(longest,
                    Vector3.Distance(tile.transform.position, neighbor.transform.position));
            }
        }
        return Mathf.Max(longest, 0.001f);
    }

    private List<HexTile> ReconstructPath(Dictionary<HexTile, HexTile> previous, HexTile last)
    {
        List<HexTile> path = new() { last };
        while (previous.TryGetValue(last, out HexTile from))
        {
            last = from;
            path.Add(last);
        }
        path.Reverse();
        return path;
    }

    // An enemy may be the FINAL tile (Battle will handle engagement).
    // No occupied tile may be used as a pass-through node.
    private bool CanTraverse(HexTile tile, HexTile goal)
    {
        if (!tile || IsNoWalk(tile))
            return false;

        if (!IsOccupied(tile)) return true;
        if (tile != goal || !IsEnemyOnTile(tile)) return false;

        // Non-aggressive AI must never plan a fight against Wilderness.
        return controllerMode == Enum_AI_Contoller_Modes.aggressive ||
               !IsWilderness(tile.CurrentUnit);
    }

    private bool IsOccupied(HexTile tile) => tile && tile.CurrentUnit && tile.CurrentUnit != this;

    // The step cost depends on both the departure and destination terrain.
    // This matches the existing leader-trait penalty calculation, while
    // allowing A* to correctly evaluate moves later in the path.
    private bool IsNoWalk(HexTile tile)
    {
        Hero leader = Leader();
        return !tile || leader == null || leader.heroClass == null ||
               leader.heroClass.leaderTraits.ContainsNoWalkType(tile);
    }

    private bool IsUnfavored(HexTile tile)
    {
        Hero leader = Leader();
        return tile && leader != null && leader.heroClass != null &&
               leader.heroClass.leaderTraits.ContainsUnfavoredType(tile);
    }

    private int GetMoveCostForTile(HexTile from, HexTile to)
    {
        if (!from || IsNoWalk(to)) return -1;

        // Preserve the existing HexWalker terrain pricing exactly.
        int departurePenalty = IsUnfavored(from) ? 1 : 0;
        int destinationCost = IsUnfavored(to) ? 2 : 1;
        return destinationCost + departurePenalty;
    }

    private bool IsAllyOnTile(HexTile tile) =>
        IsOccupied(tile) && tile.CurrentUnit.Team.CompareAllied(Team);

    private bool IsEnemyOnTile(HexTile tile) =>
        IsOccupied(tile) && !tile.CurrentUnit.Team.CompareAllied(Team);

    // -------- Mode selection (reused) --------

    public void GenerateMode()
    {
        if (CurrentTile is TeamBase)
            controllerMode = Enum_AI_Contoller_Modes.defender;
        else
            controllerMode = GetRandomMode();
    }

    public Enum_AI_Contoller_Modes GetRandomMode()
    {
        if (controllerMode != Enum_AI_Contoller_Modes.random)
            return controllerMode;

        return Random.Range(0, 3) switch
        {
            1 => Enum_AI_Contoller_Modes.aggressive,
            2 => Enum_AI_Contoller_Modes.defender,
            _ => Enum_AI_Contoller_Modes.march,
        };
    }
}
