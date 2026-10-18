using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Events;

public class UI_ChangeMoveFilters : MonoBehaviour
{
    [SerializeField] UI_SkillSlotOption display;
    [SerializeField] UI_NewFilterButton[] buttons;

    public static UnityAction OnNewCurrent;

    private void Awake()
    {
        display = GetComponentInChildren<UI_SkillSlotOption>();
        buttons = GetComponentsInChildren<UI_NewFilterButton>();

        OnNewCurrent += UpdateButtons;
    }
    private void OnDestroy()
    {
        OnNewCurrent -= UpdateButtons;
    }

    public static MovePriorityEntry CurrentPlan { get; private set; }
    public static void SetCurrentPlan(MovePriorityEntry plan)
    {
        CurrentPlan = plan;

        OnNewCurrent?.Invoke();
        UI_FullArmyDisplay.OnUpdateHero?.Invoke(UI_FullArmyDisplay.CurrentHero);
    }
    public static void ChangePlanMove(BattleMove move)
    {
        if(CurrentPlan != null)
            CurrentPlan.desiredMove = move;

        OnNewCurrent?.Invoke();
        UI_FullArmyDisplay.OnUpdateHero?.Invoke(UI_FullArmyDisplay.CurrentHero);
    }
    public static void ChangeFilter(FilterRule next)
    {
        if (next == null || CurrentPlan == null) return;

        if(CurrentContainsFilterType(next.type))
        { 
            UpdateExistingFilter(next); 
        }
        else
        {
            if(CurrentPlan.filters.Contains(next)) 
            {
                CurrentPlan.filters.Remove(next);
            }
            else
            {
                if (CurrentPlan.filters.Count >= 2)
                    CurrentPlan.filters.RemoveAt(0);

                CurrentPlan.filters.Add(next);
            }

        }  

        OnNewCurrent?.Invoke();
        UI_FullArmyDisplay.OnUpdateHero?.Invoke(UI_FullArmyDisplay.CurrentHero);
    }
    public static bool CurrentContainsFilterType(FilterRuleType rule)
    {
        if (CurrentPlan == null) return false;

        foreach (var item in CurrentPlan.filters)
            if (item.type == rule) 
                return true;

        return false;
    }
    public static bool CurrentContainsExactFilter(FilterRule rule)
    {
        if (CurrentPlan == null) return false;

        foreach (var item in CurrentPlan.filters)
            if (item.type == rule.type && item.value == rule.value) 
                return true;

        return false;
    }
    public static void UpdateExistingFilter(FilterRule rule)
    {
        if (CurrentPlan == null) return;

        for (int i = 0; i < CurrentPlan.filters.Count; i++)
        {
            if (CurrentPlan.filters[i].type != rule.type)
                continue;

            if (CurrentPlan.filters[i].value != rule.value)
            {
                CurrentPlan.filters[i] = rule;
                return;
            }
            else
            {
                CurrentPlan.filters.RemoveAt(i);
                return;
            }
        }
    } 
    public void UpdateButtons()
    {
        if(display) display.PassObject(CurrentPlan);

        foreach (var item in buttons)
            item.SetFilterDescription();
    }
}
