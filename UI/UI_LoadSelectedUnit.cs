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
}
