using UnityEngine;

public class UI_Unit_HeroLevel : ComponentLoader
{
    public bool showRank = false;
    public override void PassObject(object obj)
    {
        if (obj is Hero hero)
        {
            gameObject.SetActive(true);

            if (showRank)
            {
                SetText(hero.heroClass.rank.ToString());
                SetImageColor(RankColor(hero.heroClass.rank));
            }
            else
            { 
                SetText("lv." + hero.level.ToString()); 
                SetImageFill((float)hero.experience / (float)hero.nextLevelRequirement);
            }
        }

        else
        {
            gameObject.SetActive(false);
            SetText("--");
        }
    }
    public static Color RankColor(int rank)
    {
        return rank switch
        {
            2 => (Color.silver / 2f) + (Color.skyBlue / 2f),
            3 => (Color.gold * 0.75f) + (Color.softRed * 0.25f),
            4 => new Color(112f / 255f, 209f / 255f, 244f / 255f, 1),
            _ => Color.gray3,
        };
    }
}
