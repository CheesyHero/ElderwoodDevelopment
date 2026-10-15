using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "NMD/Game Rewards")]
public class GameRewards : ScriptableObject
{
    [Header("Hero Pools")]
    public HeroClass[] heroesCommon;
    public HeroClass[] heroesUncommon;
    public HeroClass[] heroesRare;
    public HeroClass[] heroesMythic;

    [Header("Item Pools (Reward Packages)")]
    public RewardPackage[] itemsCommon;
    public RewardPackage[] itemsUncommon;
    public RewardPackage[] itemsRare;
    public RewardPackage[] itemsMythic;

    [Header("Command Rewards")]
    public CommandCard[] commandsCommon;
    public CommandCard[] commandsUncommon;
    public CommandCard[] commandsRare;
    public CommandCard[] commandsMythic;

    [Header("Rarity Chances (Regular Rewards)")]
    [Range(0f, 1f)] public float commonChance = 0.60f;
    [Range(0f, 1f)] public float uncommonChance = 0.30f;
    [Range(0f, 1f)] public float rareChance = 0.09f;
    [Range(0f, 1f)] public float mythicChance = 0.01f;
    public Item[] Get10StoreItems(int? seed = null)
    {
        var rng = CreateRng(seed);

        var picked = new HashSet<Item>();
        var results = new List<Item>(10);

        bool AddFromPool(RewardPackage[] pool, int count)
        {
            int guard = 0;
            while (count > 0 && guard++ < 400)
            {
                if (TryPickDistinctItemFromPackages(rng, pool, picked, out Item it))
                {
                    results.Add(it);
                    count--;
                }
                else return false; 
            } 
            return count == 0;
        }
         
        AddFromPool(itemsCommon, 4); 
        AddFromPool(itemsUncommon, 4); 
        AddFromPool(itemsRare, 1); 
        bool mythic = rng.NextDouble() < 0.10;
        AddFromPool(mythic ? itemsMythic : itemsRare, 1); 
        return results.ToArray();
    } 
    public HeroClass[] Get3RandomHeroes(int? seed = null)
    {
        var rng = CreateRng(seed);
        return Get3RandomHeroes(rng);
    } 
    public HeroClass[] Get3RareRandomHeroes(int? seed = null)
    {
        var rng = CreateRng(seed);
        return Get3RareRandomHeroes(rng);
    }

    public Item[] Get3RandomItems(int? seed = null)
    {
        var rng = CreateRng(seed);
        return Get3RandomItems(rng);
    } 
    public Item[] Get3RareRandomItems(int? seed = null)
    {
        var rng = CreateRng(seed);
        return Get3RareRandomItems(rng);
    } 
    private HeroClass[] Get3RandomHeroes(System.Random rng)
    {
        var picked = new HashSet<HeroClass>();
        var results = new List<HeroClass>(3);

        int guard = 0;
        while (results.Count < 3 && guard++ < 200)
        {
            var pool = RollRegularPool(rng, heroesCommon, heroesUncommon, heroesRare, heroesMythic);
            if (TryPickDistinct(rng, pool, picked, out HeroClass h))
                results.Add(h);
        }

        return results.ToArray();
    } 
    private HeroClass[] Get3RareRandomHeroes(System.Random rng)
    {
        var picked = new HashSet<HeroClass>();
        var results = new List<HeroClass>(3);

        float rareRollChance = ComputeRareRollChance(rareChance, mythicChance);

        int guard = 0;
        while (results.Count < 3 && guard++ < 200)
        {
            bool rollRare = rng.NextDouble() < rareRollChance;
            var pool = rollRare ? heroesRare : heroesMythic;

            if (TryPickDistinct(rng, pool, picked, out HeroClass h))
                results.Add(h);
        }

        return results.ToArray();
    } 
    private Item[] Get3RandomItems(System.Random rng)
    {
        var picked = new HashSet<Item>();
        var results = new List<Item>(3);

        int guard = 0;
        while (results.Count < 3 && guard++ < 300)
        {
            var packagePool = RollRegularPool(rng, itemsCommon, itemsUncommon, itemsRare, itemsMythic);
            if (TryPickDistinctItemFromPackages(rng, packagePool, picked, out Item it))
                results.Add(it);
        }

        return results.ToArray();
    }

    private Item[] Get3RareRandomItems(System.Random rng)
    {
        var picked = new HashSet<Item>();
        var results = new List<Item>(3);

        float rareRollChance = ComputeRareRollChance(rareChance, mythicChance);

        int guard = 0;
        while (results.Count < 3 && guard++ < 300)
        {
            bool rollRare = rng.NextDouble() < rareRollChance;
            var packagePool = rollRare ? itemsRare : itemsMythic;

            if (TryPickDistinctItemFromPackages(rng, packagePool, picked, out Item it))
                results.Add(it);
        }

        return results.ToArray();
    }
    public CommandCard[] Get3RandomCommands(int? seed = null)
    {
        var rng = CreateRng(seed);
        return Get3RandomCommands(rng);
    }

    public CommandCard[] Get3RareRandomCommands(int? seed = null)
    {
        var rng = CreateRng(seed);
        return Get3RareRandomCommands(rng);
    }

    private CommandCard[] Get3RandomCommands(System.Random rng)
    {
        var picked = new HashSet<CommandCard>();
        var results = new List<CommandCard>(3);

        int guard = 0;
        while (results.Count < 3 && guard++ < 200)
        {
            var pool = RollRegularPool(rng, commandsCommon, commandsUncommon, commandsRare, commandsMythic);
            if (TryPickDistinct(rng, pool, picked, out CommandCard c))
                results.Add(c);
        }

        return results.ToArray();
    }

    private CommandCard[] Get3RareRandomCommands(System.Random rng)
    {
        var picked = new HashSet<CommandCard>();
        var results = new List<CommandCard>(3);

        float rareRollChance = ComputeRareRollChance(rareChance, mythicChance);

        int guard = 0;
        while (results.Count < 3 && guard++ < 200)
        {
            bool rollRare = rng.NextDouble() < rareRollChance;
            var pool = rollRare ? commandsRare : commandsMythic;

            if (TryPickDistinct(rng, pool, picked, out CommandCard c))
                results.Add(c);
        }

        return results.ToArray();
    }
    public List<CommandCard> Get10StoreCommands(int? seed = null)
    {
        var rng = CreateRng(seed);

        var picked = new HashSet<CommandCard>();
        var results = new List<CommandCard>(10);

        bool AddFromPool(CommandCard[] pool, int count)
        {
            int guard = 0;
            while (count > 0 && guard++ < 400)
            {
                if (TryPickDistinct(rng, pool, picked, out CommandCard cmd))
                {
                    results.Add(cmd);
                    count--;
                }
                else return false;
            }
            return count == 0;
        }

        AddFromPool(commandsCommon, 4);
        AddFromPool(commandsUncommon, 4);
        AddFromPool(commandsRare, 1);

        bool mythic = rng.NextDouble() < 0.10;
        AddFromPool(mythic ? commandsMythic : commandsRare, 1);

        return results;
    }

    private static System.Random CreateRng(int? seed)
    {
        return seed.HasValue ? new System.Random(seed.Value) : new System.Random();
    }

    private static float ComputeRareRollChance(float rareSetting, float mythicSetting)
    {
        float denom = rareSetting + mythicSetting;
        if (denom <= 0f) return 1f; // default to rare if misconfigured
        return rareSetting / denom;
    }

    private T[] RollRegularPool<T>(System.Random rng, T[] common, T[] uncommon, T[] rare, T[] mythic)
    { 
        double roll = rng.NextDouble();

        double c = commonChance;
        double u = c + uncommonChance;
        double r = u + rareChance;
        double m = r + mythicChance;

        if (roll < c) return common;
        if (roll < u) return uncommon;
        if (roll < r) return rare;
         
        return mythic;
    }

    private static bool TryPickDistinct<T>(System.Random rng, T[] pool, HashSet<T> picked, out T result) where T : class
    {
        result = null;
        if (pool == null || pool.Length == 0) return false;
         
        for (int i = 0; i < 12; i++)
        {
            var candidate = pool[rng.Next(0, pool.Length)];
            if (candidate == null) continue;

            if (picked.Add(candidate))
            {
                result = candidate;
                return true;
            }
        }
         
        for (int i = 0; i < pool.Length; i++)
        {
            var candidate = pool[i];
            if (candidate == null) continue;

            if (picked.Add(candidate))
            {
                result = candidate;
                return true;
            }
        }

        return false;
    }

    private static bool TryPickDistinctItemFromPackages(
        System.Random rng,
        RewardPackage[] packages,
        HashSet<Item> picked,
        out Item result)
    {
        result = null;
        if (packages == null || packages.Length == 0) return false;
         
        for (int attempt = 0; attempt < 18; attempt++)
        {
            var pack = packages[rng.Next(0, packages.Length)];
            if (pack == null || pack.itemPool == null || pack.itemPool.Length == 0) continue;
             
            var candidate = pack.itemPool[rng.Next(0, pack.itemPool.Length)];
            if (candidate == null) continue;

            if (picked.Add(candidate))
            {
                result = candidate;
                return true;
            }
        }
         
        for (int p = 0; p < packages.Length; p++)
        {
            var pack = packages[p];
            if (pack == null || pack.itemPool == null || pack.itemPool.Length == 0) continue;

            for (int i = 0; i < 8; i++)
            {
                var candidate = pack.itemPool[rng.Next(0, pack.itemPool.Length)];
                if (candidate == null) continue;

                if (picked.Add(candidate))
                {
                    result = candidate;
                    return true;
                }
            }
        }

        return false;
    }
    public List<HeroClass> Get3Recruits()
    {
        var rng = CreateRng(null);

        var picked = new HashSet<HeroClass>();
        var results = new List<HeroClass>(3);

        int guard = 0;
        while (results.Count < 3 && guard++ < 200)
        {
            var pool = RollRegularPool(rng, heroesCommon, heroesUncommon, heroesRare, heroesMythic);
            if (TryPickDistinct(rng, pool, picked, out HeroClass hero))
                results.Add(hero);
        }

        // Fallback in case rarity rolls or pool sizes make it impossible during normal attempts
        if (results.Count < 3)
        {
            HeroClass[][] allPools = { heroesCommon, heroesUncommon, heroesRare, heroesMythic };

            foreach (var pool in allPools)
            {
                if (results.Count >= 3) break;
                if (pool == null) continue;

                for (int i = 0; i < pool.Length; i++)
                {
                    var hero = pool[i];
                    if (hero == null) continue;

                    if (picked.Add(hero))
                        results.Add(hero);

                    if (results.Count >= 3) break;
                }
            }
        }

        return results;
    }
}
