using UnityEngine;

[CreateAssetMenu(fileName = "New Unit", menuName = "NMD/Encounters/Unit Generator")]
public class UnitGenerator : ScriptableObject
{ 
    public int leaderIndex = 0;
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

        newUnit.leaderIndex = Mathf.Clamp(leaderIndex, 0, newUnit.heroes.Count);
        newUnit.maxPlacements = newUnit.heroes.Count; 
        newUnit.debugTestSettings = (Vector3[])placements.Clone();  

        newUnit.Initialize(true);

        return newUnit;
    }
}