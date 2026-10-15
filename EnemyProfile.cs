using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Enemy Profile", menuName = "NMD/Enemy Profile")]
public class EnemyProfile : ScriptableObject
{
    public string profileName = "new profile";
    public Team team;
    public ArmyGenerator[] encounters;
    public ArmyGenerator[] bosses;
    public ArmyGenerator GetLevelEncounter(int level)
    {
        if (encounters == null || encounters.Length == 0)
            return null;

        level = Mathf.Clamp(level, 0, encounters.Length - 1);

        return encounters[level];
    }

    public ArmyGenerator GetLevelBoss(int level)
    {
        if (level < 0) level = 0;
        return bosses[level % bosses.Length];
    }
}
