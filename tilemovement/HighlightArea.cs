using System.Collections.Generic;
using UnityEngine;

public static class HighlightArea
{
    /// <summary>
    /// Highlights tiles around an origin within "range".
    /// MOVEMENT MODE (agent != null):
    ///   - Uses steps = min(range, agent.walkDistance)
    ///   - Enterable only if tile.cost <= 3
    ///   - Height: can step up <= agent.jumpHeight; step down <= agent.jumpHeight * 2
    ///   - Ignores tiles with other units on them (GetCurrentAgent() != null), except the origin
    /// ATTACK MODE (agent == null):
    ///   - Uses steps = range
    ///   - Ignores only Wall and None tiles (Empty is allowed)
    /// Returns the list of tiles that were highlighted.
    /// </summary>
    public static List<WalkSpace> Highlight(WalkSpace origin, int range, WalkAgent agent = null, bool clearExisting = true)
    {
        var result = new List<WalkSpace>();
        if (origin == null || range < 0) return result;

        if (clearExisting) WalkSpace.ResetAllSpaces();

        bool movementMode = (agent != null);
        int maxSteps = movementMode ? Mathf.Min(range, Mathf.Max(0, agent.walkDistance)) : range;

        // Standard BFS over adjacency graph
        var q = new Queue<WalkSpace>();
        var dist = new Dictionary<WalkSpace, int>();
        var visited = new HashSet<WalkSpace>();

        q.Enqueue(origin);
        dist[origin] = 0;
        visited.Add(origin);

        while (q.Count > 0)
        {
            var current = q.Dequeue();
            int d = dist[current];

            // Collect highlight candidates (skip highlighting the origin; include if desired)
            if (current != origin && d <= maxSteps)
            {
                current.SetSelectionType(E_WalkSpace_SelectionType.highlighted);
                result.Add(current);
            }

            // Stop expanding beyond maxSteps
            if (d == maxSteps) continue;

            var neighbors = current.GetAdjacent;
            if (neighbors == null) continue;

            for (int i = 0; i < neighbors.Length; i++)
            {
                var next = neighbors[i];
                if (next == null || visited.Contains(next)) continue;

                if (movementMode)
                {
                    if (!IsEnterableForMovement(origin, current, next, agent)) continue;
                }
                else
                {
                    if (!IsEnterableForAttack(next)) continue;
                }

                visited.Add(next);
                dist[next] = d + 1;
                q.Enqueue(next);
            }
        }

        return result;
    }

    /// <summary>
    /// Movement rules:
    ///  - next.cost <= 3
    ///  - Height delta up <= agent.jumpHeight; down <= agent.jumpHeight * 2
    ///  - next is not occupied, except we allow 'origin' if it happens to be occupied
    /// </summary>
    private static bool IsEnterableForMovement(WalkSpace origin, WalkSpace from, WalkSpace next, WalkAgent agent)
    {
        if (next == null) return false;

        // Cost gate
        if (next.cost > 3) return false;

        // Height gate (compare Y difference)
        float dy = next.transform.position.y - from.transform.position.y;
        if (dy > 0f)
        {
            if (dy > agent.jumpHeight + 0.01f) return false; // small epsilon
        }
        else if (dy < 0f)
        {
            if (-dy > (agent.jumpHeight * 2) + 0.01f) return false;
        }

        // Occupancy gate (allow origin even if occupied)
        if (next != origin && next.GetCurrentAgent() != null) return false;

        // Mode safety (treat wall/none/empty as not enterable regardless of cost mapping, if desired)
        var mode = next.GetSpaceMode();
        if (mode == E_WalkSpace_Mode.wall || mode == E_WalkSpace_Mode.none) return false;

        return true;
    }

    /// <summary>
    /// Attack range rules:
    ///  - Only exclude Wall and None tiles (Empty is allowed)
    ///  - No height, cost, or occupancy restrictions
    /// </summary>
    private static bool IsEnterableForAttack(WalkSpace next)
    {
        if (next == null) return false;
        var mode = next.GetSpaceMode();
        if (mode == E_WalkSpace_Mode.wall || mode == E_WalkSpace_Mode.none) return false;
        return true;
    }

    // --- Convenience overloads ---

    /// <summary>
    /// Movement preview: highlight reachable movement tiles using the agent's stats.
    /// </summary>
    public static List<WalkSpace> HighlightMovement(WalkSpace origin, WalkAgent agent, int rangeOverride = -1, bool clearExisting = true)
        => Highlight(origin, rangeOverride >= 0 ? rangeOverride : (agent?.walkDistance ?? 0), agent, clearExisting);

    /// <summary>
    /// Attack preview: highlight tiles within range ignoring only Wall/None.
    /// </summary>
    public static List<WalkSpace> HighlightAttack(WalkSpace origin, int range, bool clearExisting = true)
        => Highlight(origin, range, null, clearExisting);
}
