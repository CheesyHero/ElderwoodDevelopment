using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New World", menuName = "NMD/World")]
public class World : ScriptableObject
{
    public string worldName = "new world";

    [TextArea(2, 8)]
    public string worldDescription = "hello world!";

    [Range(1, 5)]
    public int difficulty = 1;

    [Header("Army Generators")]
    public ArmyGenerator tutorial;
    public ArmyGenerator[] earlyGameProfiles;
    public ArmyGenerator[] lateGameProfiles;
    public ArmyGenerator[] bosses;

    [Header("World Info")]
    public World nextWorld;

    [Header("Map Generation")]
    public int[] mapIndexes;
    public RunMapGenerator.Settings mapSettings = new();

    public ArmyGenerator GetTutorial()
    {
        return tutorial;
    }

    public ArmyGenerator GetRandomProfile(int mapIndex)
    {
        ArmyGenerator[] profiles;

        if (mapIndex <= 2)
        {
            profiles = earlyGameProfiles;
        }
        else if (mapIndex <= 4)
        {
            profiles = lateGameProfiles;
        }
        else
        {
            profiles = bosses;
        }

        if (profiles == null || profiles.Length == 0)
            return null;

        return profiles[Random.Range(0, profiles.Length)];
    }

    public ArmyGenerator GetRandomBoss()
    {
        if (bosses == null || bosses.Length == 0)
            return null;

        return bosses[Random.Range(0, bosses.Length)];
    }

    private List<int> shufflePool;

    public int GetRandomSceneIndex()
    {
        if (mapIndexes == null || mapIndexes.Length == 0)
            return 1;

        if (shufflePool == null || shufflePool.Count == 0)
        {
            shufflePool = new List<int>(mapIndexes);
            Shuffle(shufflePool);
        }

        int value = shufflePool[^1];
        shufflePool.RemoveAt(shufflePool.Count - 1);

        return value;
    }

    private void Shuffle(List<int> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int swapIndex = Random.Range(i, list.Count);

            (list[i], list[swapIndex]) =
                (list[swapIndex], list[i]);
        }
    }
}