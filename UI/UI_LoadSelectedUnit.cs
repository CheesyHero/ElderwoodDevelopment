using UnityEngine;

public class UI_LoadSelectedUnit : UI_UnitPreviewLoader
{
    protected void Awake()
    {
        UI_FullArmyDisplay.OnUpdateUnit += UpdateOnNew; 
    }
    private void OnDestroy()
    {
        UI_FullArmyDisplay.OnUpdateUnit -= UpdateOnNew;
    } 
    public void UpdateOnNew(Unit next)
    {
        LoadUnit(next, false);
    } 
    public override void LoadUnit(Unit unit, bool andDisable = true)
    {
        if (unit == null)
        {
            lastInput = null;
            // SetActive(false);
            SetHeaderColor(Color.white);

            return;
        }

        lastInput = unit;

        if (andDisable) SetActive(true);
        ShowDisplays(unit);
        LoadTitle();
        LoadCount();
        SetHeaderColor(unit.team.teamColor);
    }
}
