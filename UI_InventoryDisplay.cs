using System.Collections.Generic;
using UnityEngine; 

public enum Enum_InventoryFilterMode
{
    none, weapons, armor, accessories
}
public class UI_InventoryDisplay : ComponentLoader
{
    [SerializeField]
    protected UI_EquipmentButton[] buttons;
    private void Awake()
    {
        buttons = GetComponentsInChildren<UI_EquipmentButton>();
        UI_FullArmyDisplay.OnUpdateHero += PassObject;
        UI_EquipmentButton.OnUpdatedSelection += PassAndFilterPlayerInventory;
    }
    private void OnDestroy()
    {
        UI_FullArmyDisplay.OnUpdateHero -= PassObject;
        UI_EquipmentButton.OnUpdatedSelection -= PassAndFilterPlayerInventory;
    }
    public override void PassObject(object _)
    {
        LoadInventory(GameManager.Player.inventory);
    }
    public void PassAndFilterPlayerInventory(Enum_EquipmentSlot _)
    {
        LoadInventory(GameManager.Player.inventory);
    }
    public void LoadInventory(Inventory inventory)
    {
        PrintLedger(FilterItems(inventory));
    }
    public InventorySlot[] FilterItems(Inventory inventory)
    {
        return UI_EquipmentButton.LastSelection switch
        {
            Enum_EquipmentSlot.weapon => Filter(inventory.GetLedger(), Enum_InventoryFilterMode.weapons),
            Enum_EquipmentSlot.armor => Filter(inventory.GetLedger(), Enum_InventoryFilterMode.armor),
            Enum_EquipmentSlot.accessoryA => Filter(inventory.GetLedger(), Enum_InventoryFilterMode.accessories),
            Enum_EquipmentSlot.accessoryB => Filter(inventory.GetLedger(), Enum_InventoryFilterMode.accessories),
            Enum_EquipmentSlot.inventory => Filter(inventory.GetLedger(), Enum_InventoryFilterMode.none),
            _ => inventory.GetLedger(),
        };
    }
    public InventorySlot[] Filter(InventorySlot[] list, Enum_InventoryFilterMode filter)
    {
        List<InventorySlot> finds = new();
        foreach(var item in list)
            if (FilterCurrentHero(item) && CheckAgainstFilter(item, filter))
                finds.Add(item);

        return finds.ToArray();
    }
    public bool FilterCurrentHero(InventorySlot slot)
    {
        if (UI_FullArmyDisplay.CurrentHero != null)
            return UI_FullArmyDisplay.CurrentHero.CanUseEquipment(slot.item as Equipment);

        else return false;
    }
    public bool CheckAgainstFilter(InventorySlot item, Enum_InventoryFilterMode filter)
    {
        switch(filter)
        {
            default: return true; // none
            case Enum_InventoryFilterMode.weapons:
                return item.item is Weapon;
            case Enum_InventoryFilterMode.armor:
                return item.item is Armor;
            case Enum_InventoryFilterMode.accessories:
                return item.item is not Weapon && item.item is not Armor;
        }
    }
    public void PrintLedger(InventorySlot[] items)
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            if (i < items.Length)
            {
                InventorySlot slot = items[i];
                buttons[i].LoadSlotDisplay(slot);
            }
            else buttons[i].gameObject.SetActive(false);
        }
    }
    public void HideButtons()
    {
        foreach(var item in buttons) 
            item.gameObject.SetActive(false);
    }
}
