using UnityEngine;

public class UI_HealthDisplay : ComponentLoader
{
    public override void PassObject(object obj)
    {
        if (obj is Hero hero)
            SetText(Mathf.CeilToInt(hero.CombinedAttributes.health).ToString());
        else SetText("--");
    }
}
