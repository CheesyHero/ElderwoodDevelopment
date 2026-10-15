using UnityEngine;

[CreateAssetMenu(menuName = "NMD/Reward Package")]
public class RewardPackage : ScriptableObject
{
    [Range(0, 3)]
    public int rarity = 1;

    [Header("Item Pools")]
    public Item[] itemPool; 

    public Item GetRandomItem()
    {
        if (itemPool == null || itemPool.Length == 0)
            return null;

        // FIX: upper bound is exclusive for ints; do not subtract 1
        return itemPool[Random.Range(0, itemPool.Length)];
    }

    // Seeded RNG (for GameRewards determinism)
    public Item GetRandomItem(System.Random rng)
    {
        if (itemPool == null || itemPool.Length == 0)
            return null;

        if (rng == null) rng = new System.Random();
        return itemPool[rng.Next(0, itemPool.Length)];
    }
}
