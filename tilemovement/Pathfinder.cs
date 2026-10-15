using System.Collections.Generic;
using UnityEngine;

public static class Pathfinder
{
    /// <summary>
    /// A* path from 'from' to 'to' with constraints:
    /// - Step cost is uniform (1 per edge).
    /// - Cannot enter tiles with Cost > 3.
    /// - Cannot enter occupied tiles (GetCurrentUnit() != null), except the origin.
    /// If the goal is occupied or unreachable, returns a path to the closest valid reachable tile to 'to'.
    /// </summary>
    public static Queue<WalkSpace> GetPathTo(WalkSpace from, WalkSpace to)
    {
        var empty = new Queue<WalkSpace>();
        if (from == null || to == null) return empty;

        // Trivial case
        if (from == to)
        {
            var q = new Queue<WalkSpace>();
            q.Enqueue(to);
            return q;
        }

        // A* structures
        var openList = new List<WalkSpace> { from };
        var openSet = new HashSet<WalkSpace> { from };
        var closedSet = new HashSet<WalkSpace>();

        var cameFrom = new Dictionary<WalkSpace, WalkSpace>();
        var gScore = new Dictionary<WalkSpace, float> { [from] = 0f };
        var fScore = new Dictionary<WalkSpace, float> { [from] = Heuristic(from, to) };

        // Track the best "fallback" node: the valid node we've seen that's closest (by heuristic) to the target.
        WalkSpace bestFallback = from;
        float bestFallbackH = Heuristic(from, to);

        // If the target is occupied, we will never be able to enter it; search will end on fallback.
        bool targetEnterable = IsEnterable(to, from);

        while (openList.Count > 0)
        {
            // Node in open with lowest fScore (tie-break on smaller heuristic to goal)
            WalkSpace current = GetLowestF(openList, fScore, to);

            // If we can actually enter target tiles and we reached it, done.
            if (current == to && targetEnterable)
                return ReconstructPath(cameFrom, current);

            openList.Remove(current);
            openSet.Remove(current);
            closedSet.Add(current);

            var neighbors = current.GetAdjacent;
            if (neighbors == null) continue;

            for (int i = 0; i < neighbors.Length; i++)
            {
                var neighbor = neighbors[i];
                if (neighbor == null) continue;
                if (closedSet.Contains(neighbor)) continue;

                // Enforce movement rules:
                // - Cannot step into cost>3 tiles
                // - Cannot step into occupied tiles, except the origin ('from')
                if (!IsEnterable(neighbor, from)) continue;

                float tentativeG = gScore[current] + 1f; // uniform edge cost

                if (!openSet.Contains(neighbor))
                {
                    openSet.Add(neighbor);
                    openList.Add(neighbor);
                }
                else if (gScore.TryGetValue(neighbor, out float existingG) && tentativeG >= existingG)
                {
                    // Not a better path
                    continue;
                }

                // This path is best so far for neighbor
                cameFrom[neighbor] = current;
                gScore[neighbor] = tentativeG;
                float h = Heuristic(neighbor, to);
                fScore[neighbor] = tentativeG + h;

                // Update fallback (closest valid node we've discovered wrt target)
                if (h < bestFallbackH)
                {
                    bestFallback = neighbor;
                    bestFallbackH = h;
                }

                // Early exit: if we are allowed to enter the target and this neighbor IS the target,
                // we could accept it here — but the top-of-loop check already handles equality.
            }
        }

        // Goal not reached (likely occupied/unreachable). Return path to best reachable candidate.
        if (bestFallback != null && bestFallback != from)
            return ReconstructPath(cameFrom, bestFallback);

        // No progress possible
        return empty;
    }

    /// <summary>
    /// Returns true if this tile can be entered, given our rules and origin exception.
    /// </summary>
    private static bool IsEnterable(WalkSpace tile, WalkSpace origin)
    {
        if (tile == null) return false;

        // Cost gate
        // Adjust "Cost" to match your WalkSpace API if needed (e.g., MovementCost, StepCost).
        if (tile.cost > 3) return false;

        // Occupancy gate (origin is allowed even if occupied)
        if (tile != origin && tile.GetCurrentAgent() != null) return false;

        return true;
    }

    /// <summary>
    /// Heuristic (grid-friendly). Uses Manhattan on X/Z plus a mild Y weight to discourage needless vertical moves.
    /// </summary>
    private static float Heuristic(WalkSpace a, WalkSpace b)
    {
        Vector3 ap = a.transform.position;
        Vector3 bp = b.transform.position;

        float dx = Mathf.Abs(ap.x - bp.x);
        float dy = Mathf.Abs(ap.y - bp.y);
        float dz = Mathf.Abs(ap.z - bp.z);

        return dx + dz + dy * 1.25f;
    }

    /// <summary>
    /// Pick node in open with smallest f; tie-breaker uses smaller heuristic to goal.
    /// </summary>
    private static WalkSpace GetLowestF(List<WalkSpace> open,
                                        Dictionary<WalkSpace, float> fScore,
                                        WalkSpace goal)
    {
        WalkSpace best = open[0];
        float bestF = GetOrInfinity(fScore, best);
        float bestH = Heuristic(best, goal);

        for (int i = 1; i < open.Count; i++)
        {
            var node = open[i];
            float f = GetOrInfinity(fScore, node);

            if (f < bestF)
            {
                best = node;
                bestF = f;
                bestH = Heuristic(node, goal);
            }
            else if (Mathf.Approximately(f, bestF))
            {
                float h = Heuristic(node, goal);
                if (h < bestH)
                {
                    best = node;
                    bestF = f;
                    bestH = h;
                }
            }
        }
        return best;
    }

    private static float GetOrInfinity(Dictionary<WalkSpace, float> dict, WalkSpace key)
        => dict.TryGetValue(key, out float v) ? v : float.PositiveInfinity;

    /// <summary>
    /// Rebuilds the path from start to 'current' by following cameFrom links.
    /// </summary>
    private static Queue<WalkSpace> ReconstructPath(Dictionary<WalkSpace, WalkSpace> cameFrom,
                                                    WalkSpace current)
    {
        var stack = new Stack<WalkSpace>();
        stack.Push(current);

        while (cameFrom.TryGetValue(current, out var prev))
        {
            current = prev;
            stack.Push(current);
        }

        var path = new Queue<WalkSpace>(stack.Count);
        while (stack.Count > 0) path.Enqueue(stack.Pop());
        return path;
    }
}
