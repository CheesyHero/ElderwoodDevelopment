using UnityEngine;

public class UI_UnitUIHealth : ComponentLoader
{
    public Gradient gradient;
    public override void PassObject(object obj)
    {
        if (!image) return;
        if (obj is Hero hero)
        {
            image.fillAmount = hero.health.HealthPercent;
            image.color = gradient.Evaluate(hero.health.HealthPercent);
            gameObject.SetActive(true);
        }
        else
        {
            image.fillAmount = 0;
            gameObject.SetActive(false);
        }

    }
}
