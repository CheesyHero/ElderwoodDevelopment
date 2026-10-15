using UnityEngine;

public class UI_BattlePlanDisplay : ComponentLoader
{
    UI_SkillSlotOption[] options;

    private void Awake()
    {
        options = GetComponentsInChildren<UI_SkillSlotOption>(); 

        UI_FullArmyDisplay.OnUpdateHero += PassObject;
        UI_UnitSlot.OnHoverPreviewEnter += Preview;
        UI_UnitSlot.OnHoverPreviewExit += Return;
    }
    private void OnDestroy()
    {
        UI_FullArmyDisplay.OnUpdateHero -= PassObject;
        UI_UnitSlot.OnHoverPreviewEnter -= Preview;
        UI_UnitSlot.OnHoverPreviewExit -= Return;
    }
    public override void PassObject(object obj)
    {
        if(obj is Hero hero)
        {
            for(int i = 0; i < options.Length; i++)
            {
                options[i].SetIndex(i);
                int count = hero.plan.skills.Count;
                if (i < hero.plan.skills.Count)
                    options[i].PassObject(hero.plan.skills[i]);

                else if(i - count < hero.heroClass.passives.Count)
                    options[i].PassObject(hero.heroClass.passives[i - count]);

                else if (i == count + hero.heroClass.passives.Count)
                    options[i].PassObject(hero);

                else options[i].PassObject(null);
            }
        }
        else foreach(var item in options)
                item.PassObject(null);
    }
    public void Preview(Hero h)
    {
        PassObject(h);
    }
    public void Return()
    {
        PassObject(UI_FullArmyDisplay.CurrentHero);
    }
}
