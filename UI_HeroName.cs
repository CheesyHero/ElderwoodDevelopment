using UnityEngine;
using TMPro;

public class UI_HeroName : ComponentLoader
{
    public TMP_InputField field;

    public enum E_StringPrintModes
    {
        heroName, heroClass, heroClassWithLevel, position
    }
    public E_StringPrintModes target;
    public override void PassObject(object obj)
    {
        if (field != null)
            field.text = PrintOut((Hero)obj);

        else SetText(PrintOut((Hero)obj));
    }
    public string PrintOut(Hero hero)
    {
        if (target == E_StringPrintModes.position)
            return PrintPosition();
        if (hero == null)
            return "No Hero";

        return target switch
        {
            E_StringPrintModes.heroName => hero.characterName,
            E_StringPrintModes.heroClass => hero.heroClass ? hero.heroClass.className : "Adventurer",
            E_StringPrintModes.heroClassWithLevel => $"Level <b>{hero.level}</b> {(hero.heroClass ? hero.heroClass.className : "Adventurer")}",
            // E_StringPrintModes.position=> $"({UI_UnitSlot.LastIndex.x},{UI_UnitSlot.LastIndex.y})",
            _ => hero.ToString(),
        };
    }
    public string PrintPosition()
    {
        int x = (int)UI_UnitSlot.LastIndex.x;
        int y = (int)UI_UnitSlot.LastIndex.y;

        return $"{Row(x)}\n{Column(y)}";
    }
    protected string Row(int x)
    {
        if (x == 0) return "Front";
        else return "Back";
    }
    protected string Column(int y)
    {
        if (y == 0) return "Left";
        else if (y == 1) return "Mid";
        else return "Right";
    }
}
