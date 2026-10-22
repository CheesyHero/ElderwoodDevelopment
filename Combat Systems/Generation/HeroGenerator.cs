using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Hero", menuName = "NMD/Encounters/HeroGenerator")]
public class HeroGenerator : ScriptableObject
{
    public bool useRecommendedSkills = false;
    public Hero hero;

    public Hero Copy(Team team)
    {
        var copy = hero.Copy();
        copy.RollName(true);
        copy.team = team;

        // copy.Initialize();

        if (useRecommendedSkills)
            copy.plan.GenerateSkillFresh(copy, true);

        return copy;
    }
}
