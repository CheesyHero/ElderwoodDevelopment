using UnityEngine;

[CreateAssetMenu(fileName = "New Unit", menuName = "NMD/Encounters/Unit Generator")]
public class UnitGenerator : ScriptableObject
{
    public int minLevel, maxLevel;
    public HeroGenerator[] heroes;
    public Vector3[] placements = new Vector3[] { new(-1, -1, -1), new(-1, -1, -1) };

    public Unit Copy(Team team)
    {
        // Create
        Unit newUnit = new(team); 

        foreach (var hero in heroes)
        {
            Hero copy = hero.Copy(team); 

            newUnit.heroes.Add(copy);
        }

        return ScaleUnit(newUnit);
    } 

    public Unit ScaleUnit(Unit input, bool boss = false)
    { 
        int halfCurrent = Mathf.FloorToInt(GameManager.Distance / 2f);

        input.Leader().level =
            Mathf.Clamp(1 + halfCurrent + GameManager.CurrentLevel
            + GameManager.Player.HighestLevel(0.33f)
            + Random.Range(minLevel, maxLevel), 1, 20);

        for (int i = 1; i < input.heroes.Count; i++)
            input.heroes[i].level = (boss ? GameManager.Player.currentWorld.difficulty : 0) +
                Mathf.Clamp(halfCurrent + GameManager.CurrentLevel
                + GameManager.Player.HighestLevel(0.25f)
                + Random.Range(minLevel, maxLevel), 1, 20);

        return input;
    }
}
