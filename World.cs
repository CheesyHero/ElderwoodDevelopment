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
    public EnemyProfile tutorial;
    public EnemyProfile[] availableProfiles;
    public EnemyProfile[] bosses;
    public int[] mapIndexes;

    [Header("World Info")]
    public World nextWorld;

    [Header("Map Generation")] 
    public RunMapGenerator.Settings mapSettings = new();

    public EnemyProfile GetTutorial() => tutorial;

    public EnemyProfile GetRandomProfile()
    {
        if(availableProfiles.Length == 0) return null;
        else return availableProfiles[Random.Range(0, availableProfiles.Length)];
    }
    public EnemyProfile GetRandomBoss()
    {
        if (bosses.Length == 0) return null;
        else return bosses[Random.Range(0, bosses.Length)];
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
            (list[i], list[swapIndex]) = (list[swapIndex], list[i]);
        }
    }

}
