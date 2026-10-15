using UnityEngine;

public class UI_HeroButton : ComponentLoader
{
    public override void PassObject(object obj)
    {
        if (obj is not Hero hero) return;
        SetActive(true);
        SetText($"{hero} : {(hero.heroClass ? hero.heroClass.className : "Adventuerer")}");
    }
}
