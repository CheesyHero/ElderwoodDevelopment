using TMPro;
using UnityEngine;

public class UI_SkillSlotOption : ComponentLoader
{
    public static int desiredIndex;

    MovePriorityEntry lastInput;
    Passive lastPass;
    public int index;
    public TextMeshProUGUI priority;
    public UI_MoveDisplay movebutton;
    public Color setColor, unsetColor;
    public void SetIndex(int set)
    {
        index = set;
    }
    public override void PassObject(object obj)
    {
        if (obj is MovePriorityEntry skillSlot)
        {
            lastInput = skillSlot;
            lastPass = null;

            if (movebutton) 
                movebutton.PassObject(skillSlot.desiredMove);

            SetImageColor(setColor);
            SetFilterDescriptions();
            SetIndexDisplay(Mathf.Abs(skillSlot.priority) + 1);
        }
        else if(obj is Passive passive)
        {
            lastInput = null;
            lastPass = passive;

            if (movebutton)
                movebutton.PassObject(passive);

            SetImageColor(setColor);
            SetFilterDescriptions();
            SetIndexDisplay(0);
        }
        else if(obj is Hero hero)
        {
            lastInput = null;
            lastPass = hero.heroClass.leaderTraits.passive;

            if (movebutton) movebutton.PassObject(hero);

            SetImageColor(setColor);
            SetFilterDescriptions();
            SetIndexDisplay(0);
        } 

        else PrepareNewInput();
    }
    public void TickPriority(int dir)
    {
        lastInput?.TickPriority(dir); 
        PassObject(lastInput);

        UI_FullArmyDisplay.CurrentHero?.plan.SortByPriority();
        UI_FullArmyDisplay.OnUpdateHero?.Invoke(UI_FullArmyDisplay.CurrentHero);
    }
    public void SetIndexDisplay(int setting)
    {
        if (priority) priority.text = setting.ToString();
    }

    protected void PrepareNewInput()
    {
        if (movebutton) movebutton.PassObject(null);
        SetImageColor(unsetColor);
        SetText("<color=#424242>No skill set</color>");
        SetIndexDisplay(0);
    }
    protected void SetFilterDescriptions()
    {
        if (textMesh == null) return;
        if(lastPass != null)
        {
            SetText($"<color=#b5b5b5>{lastPass.description}</color>");
            return;
        }

        if(lastInput == null)
        {
            SetText("No Filters");
            return;
        }

        SetText("");
        foreach(var item in lastInput.filters)
        {
            if(textMesh.text.Length > 1)
                textMesh.text += ";\n";
            textMesh.text += item.PrintEffectDetails();
        }

        if (textMesh.text.Length <= 1)
            SetText("No Filters");
    }
    public void PassSkillSlot()
    {
        desiredIndex = index;
        UI_ChangeMoveFilters.SetCurrentPlan(lastInput);
    } 
}
