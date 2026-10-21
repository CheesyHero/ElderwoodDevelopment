using UnityEngine;

[CreateAssetMenu(fileName = "New Unit", menuName = "NMD/Encounters/Unit Generator")]
public class UnitGenerator : ScriptableObject
{ 
    public int minLevel, maxLevel; 
    public HeroGenerator[] heroes; 
    public Vector3[] placements = new Vector3[]
    {
        new(-1, -1, -1),
        new(-1, -1, -1)
    };

    public Unit Copy(Team team)
    { 
        Unit newUnit = new(team); 

        foreach (var hero in heroes)
        {
            if (hero == null) continue;

            Hero copy = hero.Copy(team);
            newUnit.heroes.Add(copy);
        }
         
        newUnit.maxPlacements = newUnit.heroes.Count; 
        newUnit.debugTestSettings = (Vector3[])placements.Clone();  

        newUnit.Initialize(true);

        return ScaleUnit(newUnit);
    }

    public Unit ScaleUnit(Unit input, bool boss = false)
    {
        if (input == null || input.heroes.Count == 0)
            return input;

        int halfCurrent = Mathf.FloorToInt(GameManager.Distance / 2f);
         
        input.Leader().level =
            Mathf.Clamp(
                1 + halfCurrent
                + GameManager.CurrentLevel
                + GameManager.Player.HighestLevel(0.33f)
                + Random.Range(minLevel, maxLevel),
                1, 20
            );
         
        for (int i = 0; i < input.heroes.Count; i++)
        {
            if (i == input.leaderIndex) continue;

            input.heroes[i].level =
                (boss ? GameManager.Player.currentWorld.difficulty : 0)
                + Mathf.Clamp(
                    halfCurrent
                    + GameManager.CurrentLevel
                    + GameManager.Player.HighestLevel(0.25f)
                    + Random.Range(minLevel, maxLevel),
                    1, 20
                );
        }

        return input;
    }
}