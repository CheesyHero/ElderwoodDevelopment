using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class RunMapData
{
    public int seed;
    public List<MapNodeData> nodes = new();
    public List<MapEdgeData> edges = new();

    public MapNodeData GetNode(int id) => nodes.Find(n => n.id == id);
}

public enum MapNodeType
{
    Start,
    Normal,
    Boss
}

[Serializable]
public class MapEdgeData
{
    public int fromId;
    public int toId;
}

public static class RunMapGenerator
{
    [Serializable]
    public class Settings
    {
        public readonly int steps = 6;          // total columns including start and boss
        public readonly int minNodesPerStep = 2;
        public readonly int maxNodesPerStep = 3;
        public readonly int minBranchLinks = 1;  // links per node going forward
        public readonly int maxBranchLinks = 2;

        public readonly float skipStepChance = 0.10f; // chance a middle step collapses to 1 node
        public readonly float extraBranchChance = 0.35f;

        public readonly int mergeWindow = 3;     // last N steps before boss will trend to merging
        public readonly float xSpacing = 220f;
        public readonly float ySpacing = 140f;
    }

    public static RunMapData Generate(Settings s, int? fixedSeed = null)
    {
        int seed = fixedSeed ?? UnityEngine.Random.Range(int.MinValue, int.MaxValue);
        var rng = new System.Random(seed);

        var map = new RunMapData { seed = seed };

        int nextId = 0;

        // Layers by depth
        var layers = new List<List<int>>(s.steps);

        // Depth 0: Start
        layers.Add(new List<int>());
        int startId = AddNode(map, ref nextId, 0, 0f, 0f, MapNodeType.Start);
        layers[0].Add(startId);

        // Depth 1..steps-2: middle layers
        for (int d = 1; d <= s.steps - 2; d++)
        {
            int count = rng.Next(s.minNodesPerStep, s.maxNodesPerStep + 1);

            // Encourage tighter merging near the end
            int stepsFromEnd = (s.steps - 1) - d; // boss is steps-1
            if (stepsFromEnd <= s.mergeWindow)
                count = Mathf.Clamp(count - 1, 1, s.maxNodesPerStep);

            // Occasionally collapse a step
            if (rng.NextDouble() < s.skipStepChance)
                count = 1;

            layers.Add(new List<int>(count));

            // Layout: spread vertically around 0
            float x = d * s.xSpacing;
            float y0 = -((count - 1) * 0.5f) * s.ySpacing;

            for (int i = 0; i < count; i++)
            {
                float y = y0 + i * s.ySpacing;
                int id = AddNode(map, ref nextId, d, x, y, MapNodeType.Normal);
                layers[d].Add(id);
            }
        }

        // Depth steps-1: Boss
        layers.Add(new List<int>());
        int bossDepth = s.steps - 1;
        int bossId = AddNode(map, ref nextId, bossDepth, bossDepth * s.xSpacing, 0f, MapNodeType.Boss);
        layers[bossDepth].Add(bossId);

        // Connect layers
        for (int d = 0; d < s.steps - 1; d++)
        {
            var fromLayer = layers[d];
            var toLayer = layers[d + 1];

            // Forced merge into boss
            if (d == s.steps - 2)
            {
                foreach (var fromId in fromLayer)
                    AddEdgeUnique(map, fromId, bossId);
                continue;
            }

            foreach (var fromId in fromLayer)
            {
                int links = rng.Next(s.minBranchLinks, s.maxBranchLinks + 1);

                // In merge window, reduce branching
                int stepsFromEnd = (s.steps - 1) - d;
                if (stepsFromEnd <= s.mergeWindow + 1)
                    links = Mathf.Clamp(links - 1, 1, s.maxBranchLinks);

                // Occasionally add extra branch link
                if (rng.NextDouble() < s.extraBranchChance)
                    links = Mathf.Min(links + 1, s.maxBranchLinks);

                // Choose targets: bias to closer vertical neighbors for nicer looking paths
                var targets = PickTargetsBiased(rng, map, fromId, toLayer, links);
                foreach (var toId in targets)
                    AddEdgeUnique(map, fromId, toId);
            }

            // Ensure every node in toLayer is reachable (except start)
            if (d >= 0)
                EnsureReachable(map, fromLayer, toLayer, rng);
        }

        return map;
    }
    private static int AddNode(
    RunMapData map,
    ref int nextId,
    int depth,
    float x,
    float y,
    MapNodeType type)
    {
        int id = nextId++;

        World world = LevelSelectMap.World;

        if (world == null)
        {
            Debug.LogError("RunMapGenerator: No World assigned.");
            return id;
        }

        ArmyGenerator army =
            depth <= 0 && world.GetTutorial()
                ? world.GetTutorial()
                : world.GetRandomProfile(depth);

        if (army == null)
        {
            Debug.LogError(
                $"RunMapGenerator: No ArmyGenerator found for map depth {depth}."
            );

            return id;
        }

        var next = new MapNodeData
        {
            id = id,
            depth = depth,
            position = new Vector2(x, y),
            type = type,
            main = army,
            completed = depth == 0,
            sceneIndex = world.GetRandomSceneIndex(),

            storeInventory = army.GetStore(),

            cardMarket = army.rewards != null
                ? army.rewards.Get10StoreCommands()
                : null,

            recruits = army.rewards != null
                ? army.rewards.Get3Recruits()
                : null
        };

        if (next.depth == 0 && GameManager.CurrentLevel == 0)
            GameManager.LoadNewEncounter(next);

        map.nodes.Add(next);

        return id;
    }

    private static void AddEdgeUnique(RunMapData map, int fromId, int toId)
    {
        for (int i = 0; i < map.edges.Count; i++)
        {
            if (map.edges[i].fromId == fromId && map.edges[i].toId == toId)
                return;
        }

        map.edges.Add(new MapEdgeData { fromId = fromId, toId = toId });

        var fromNode = map.GetNode(fromId);
        if (fromNode != null && !fromNode.nextNodeIds.Contains(toId))
            fromNode.nextNodeIds.Add(toId);
    }

    private static List<int> PickTargetsBiased(System.Random rng, RunMapData map, int fromId, List<int> candidates, int count)
    {
        var result = new List<int>(count);
        if (candidates.Count == 0) return result;

        float fromY = map.GetNode(fromId).position.y;

        // Sort candidates by vertical distance to fromY
        var sorted = new List<int>(candidates);
        sorted.Sort((a, b) =>
        {
            float da = Mathf.Abs(map.GetNode(a).position.y - fromY);
            float db = Mathf.Abs(map.GetNode(b).position.y - fromY);
            return da.CompareTo(db);
        });

        // Weighted pick: prefer the closest few
        int pool = Mathf.Min(sorted.Count, 3);
        while (result.Count < count && result.Count < candidates.Count)
        {
            int pickIndex = rng.Next(0, pool);
            int pick = sorted[pickIndex];
            if (!result.Contains(pick))
                result.Add(pick);
            pool = Mathf.Min(sorted.Count, pool + 1);
        }

        // Guarantee at least one
        if (result.Count == 0)
            result.Add(sorted[0]);

        return result;
    }

    private static void EnsureReachable(RunMapData map, List<int> fromLayer, List<int> toLayer, System.Random rng)
    {
        // For each node in toLayer, ensure at least one incoming edge from fromLayer.
        foreach (int toId in toLayer)
        {
            bool hasIncoming = false;
            for (int i = 0; i < map.edges.Count; i++)
            {
                if (map.edges[i].toId == toId)
                {
                    hasIncoming = true;
                    break;
                }
            }

            if (!hasIncoming && fromLayer.Count > 0)
            {
                int fromId = fromLayer[rng.Next(0, fromLayer.Count)];
                AddEdgeUnique(map, fromId, toId);
            }
        }
    }
}
