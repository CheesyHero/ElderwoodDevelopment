using UnityEditor;
using UnityEngine;

public class UI_ActionPointDisplay : ComponentLoader
{
    public enum Modes { actionPoints, reactionPoints }
    public Modes mode; 
    public GameObject[] children; 

    public override void PassObject(object obj)
    {
        if (obj is Hero hero)
        {
            SetChildren(hero);
            SetText(GetValue(hero));
        }
        else SetText("-");
    } 
    public void SetChildren(Hero hero)
    {
        if (children.Length <= 0) return;

        for (int i = 0; i < children.Length; i++)
        {
            var child = children[i];
            child.SetActive(GetFromMode(hero) > i);
        }
    }
    public string GetValue(Hero hero)
    {
        if (hero == null) return "-";
        else return (mode == Modes.actionPoints ? hero.CombinedAttributes.actions : hero.CombinedAttributes.reactions).ToString();
    }
    public int GetFromMode(Hero hero)
    {
        if (hero == null) return 0;
        else return mode == Modes.actionPoints ? hero.CombinedAttributes.actions : hero.CombinedAttributes.reactions;
    }
}
