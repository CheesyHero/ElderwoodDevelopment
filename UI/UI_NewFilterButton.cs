using UnityEngine; 

public class UI_NewFilterButton : ComponentLoader
{ 
    public FilterRule filter;

    public override void PassObject(object obj)
    {
        SetFilterDescription();
    }

    private void OnEnable()
    {
        SetFilterDescription();
    }

    public void SetFilterDescription()
    {  
        SetText(filter.PrintEffectDetails());
    }

    public void PassFilter()
    {
        UI_ChangeMoveFilters.ChangeFilter(filter);
    }
}
