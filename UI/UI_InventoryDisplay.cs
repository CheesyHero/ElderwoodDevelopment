
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
        buttons = GetComponentsInChildren<UI_EquipmentButton>(true);

        // Inventory buttons should always use the inventory slot type.
        foreach (UI_EquipmentButton button in buttons)
            button.slot = Enum_EquipmentSlot.inventory;
    }

    private void OnEnable()
    {
        UI_FullArmyDisplay.OnUpdateHero += PassObject;
        UI_EquipmentButton.OnUpdatedSelection += PassAndFilterPlayerInventory;

        RefreshInventory();
    }

    private void OnDisable()
    {
        UI_FullArmyDisplay.OnUpdateHero -= PassObject;
        UI_EquipmentButton.OnUpdatedSelection -= PassAndFilterPlayerInventory;
    }

    public override void PassObject(object _)
    {
        RefreshInventory();
    }

    public void PassAndFilterPlayerInventory(Enum_EquipmentSlot _)
    {
        RefreshInventory();
    }

    public void RefreshInventory()
    {
        if (GameManager.Player == null)
        {
            HideButtons();
            return;
        }

        LoadInventory(GameManager.Player.inventory);
    }

    public void LoadInventory(Inventory inventory)
    {
        if (inventory == null)
        {
            HideButtons();
            return;
        }

        PrintLedger(FilterItems(inventory));
    }

    public InventorySlot[] FilterItems(Inventory inventory)
    {
        InventorySlot[] ledger = inventory.GetLedger();

        Enum_InventoryFilterMode filter = UI_EquipmentButton.LastSelection switch
        {
            Enum_EquipmentSlot.weapon => Enum_InventoryFilterMode.weapons,
            Enum_EquipmentSlot.armor => Enum_InventoryFilterMode.armor,
            Enum_EquipmentSlot.accessoryA => Enum_InventoryFilterMode.accessories,
            Enum_EquipmentSlot.accessoryB => Enum_InventoryFilterMode.accessories,
            _ => Enum_InventoryFilterMode.none
        };

        return Filter(ledger, filter);
    }

    public InventorySlot[] Filter(InventorySlot[] list, Enum_InventoryFilterMode filter)
    {
        List<InventorySlot> finds = new();

        if (list == null)
            return finds.ToArray();

        foreach (InventorySlot slot in list)
        {
            if (slot == null || slot.item == null || slot.amount <= 0)
                continue;

            if (!CheckAgainstFilter(slot, filter))
                continue;

            if (filter == Enum_InventoryFilterMode.none || FilterCurrentHero(slot))
                finds.Add(slot);
        }

        return finds.ToArray();
    }

    public bool FilterCurrentHero(InventorySlot slot)
    {
        Hero hero = UI_FullArmyDisplay.CurrentHero;

        if (hero == null || slot?.item is not Equipment equipment)
            return false;

        return hero.CanUseEquipment(equipment);
    }

    public bool CheckAgainstFilter(InventorySlot slot, Enum_InventoryFilterMode filter)
    {
        return filter switch
        {
            Enum_InventoryFilterMode.weapons => slot.item is Weapon,
            Enum_InventoryFilterMode.armor => slot.item is Armor,
            Enum_InventoryFilterMode.accessories => slot.item is Equipment
                && slot.item is not Weapon
                && slot.item is not Armor,
            _ => true
        };
    }

    public void PrintLedger(InventorySlot[] items)
    {
        if (buttons == null)
            return;

        for (int i = 0; i < buttons.Length; i++)
        {
            UI_EquipmentButton button = buttons[i];

            if (button == null)
                continue;

            bool hasItem = i < items.Length;

            if (hasItem)
            {
                button.gameObject.SetActive(true);
                button.LoadSlotDisplay(items[i]);
            }
            else
            {
                button.gameObject.SetActive(false);
            }
        }
    }

    public void HideButtons()
    {
        if (buttons == null)
            return;

        foreach (UI_EquipmentButton button in buttons)
        {
            if (button != null)
                button.gameObject.SetActive(false);
        }
    }
}
