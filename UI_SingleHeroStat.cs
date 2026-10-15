using UnityEngine;

public class UI_SingleHeroStat : ComponentLoader
{
    public Enum_HeroAttributes target;
    public override void PassObject(object obj)
    {
        if (obj is Hero hero)
            SetText(LoadText() + Colorize(GetValue(hero), hero));

        else SetText(LoadText() + Colorize(GetValue(null), null));
    }
    public string Colorize(string input, Hero hero)
    {
        if (hero == null) return $"<color=#525252>{input}</color>";

        float difference = hero.CombinedAttributes.GetStat(target) - hero.attributes.GetStat(target);
        if (difference == 0) return input;
        else if (difference > 0) return $"<color=#0aa319>{input}</color>";
        else return $"<color=#a60a24>{input}</color>";
    }
    public string LoadText()
    {
        return Attributes.PrintAttributeName(target) + " : ";
    }
    public string GetValue(Hero hero)
    {
        if (hero == null) return "--";

        return Mathf.RoundToInt(hero.CombinedAttributes.GetStat(target)).ToString();

    }
}
