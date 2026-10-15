using UnityEngine;

public class UI_EquipmentArray : ComponentLoader
{
    [SerializeField]
    protected UI_EquipmentButton[] buttons;

    private void Awake()
    {
        buttons = GetComponentsInChildren<UI_EquipmentButton>();
        UI_FullArmyDisplay.OnUpdateHero += PassObject;

        UI_UnitSlot.OnHoverPreviewEnter += Preview;
        UI_UnitSlot.OnHoverPreviewExit += Return;
    }
    private void OnDestroy()
    {
        UI_UnitSlot.OnHoverPreviewEnter -= Preview;
        UI_UnitSlot.OnHoverPreviewExit -= Return;
    }

    public override void PassObject(object obj)
    {
        foreach(var item in buttons) 
            item.PassObject(obj);
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
