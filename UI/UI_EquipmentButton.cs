using UnityEngine;
using UnityEngine.Events; 
using UnityEngine.EventSystems; 
using UnityEngine.UI; 

public enum Enum_EquipmentSlot
{
    weapon, armor, accessoryA, accessoryB, inventory
}
[SelectionBase]
public class UI_EquipmentButton : ComponentLoader, IPointerEnterHandler, IPointerExitHandler
{
    public Vector3 tooltipOffset = new(0, 50);

    [Header("Main")]
    public Button button;
    public Image icon;
    protected InventorySlot lastSlot;
    protected Equipment lastItem;
    public static Enum_EquipmentSlot LastSelection { get; private set; }
    public Enum_EquipmentSlot slot;
    public static UnityAction<Enum_EquipmentSlot> OnUpdatedSelection;

    private void Start()
    {
        UI_FullArmyDisplay.OnUpdateHero += PassObject;
        UI_UnitSlot.OnHoverPreviewEnter += PassObject;
        UI_UnitSlot.OnHoverPreviewExit += ReturnToLast;
    }

    private void OnDestroy()
    {
        UI_FullArmyDisplay.OnUpdateHero -= PassObject;
        UI_UnitSlot.OnHoverPreviewEnter -= PassObject;
        UI_UnitSlot.OnHoverPreviewExit -= ReturnToLast;
    }

    public override void PassObject(object obj)
    {
        if (obj is Hero hero)
            LoadDisplay(PullEquipment(hero));

        else if (obj is Equipment item)
            LoadDisplay(item);

        else LoadDisplay(null);
    }
    public void ReturnToLast() => PassObject(UI_FullArmyDisplay.CurrentHero);
    public void LoadDisplay(Equipment item)
    {
        if (item == null)
            LoadEmpty();
        else
            PrintItem(item);
    }
    public void LoadSlotDisplay(InventorySlot slot)
    {
        if (slot == null)
            LoadEmpty();
        else
            PrintSlot(slot);
    }
    protected void LoadEmpty()
    {
        LoadEquipment(null, 0);
    }
    protected void PrintItem(Equipment item)
    { 
        lastSlot = null;
        lastItem = item;

        gameObject.SetActive(true);
        LoadEquipment(item, 1);
    }
    public virtual void PrintSlot(InventorySlot slot)
    { 
        lastSlot = slot;
        lastItem = null;

        gameObject.SetActive(true);
        Equipment equip = slot.item as Equipment;
        if (equip != null)
            LoadEquipment(equip, slot.amount);

        else SlotText(slot);
    }
    public virtual void SlotText(InventorySlot slot)
    {
        SetText($"{slot.item.name} - Misc. : {slot.amount}");
    }
    public virtual void EquipmentPrint(Equipment equip, int quantity)
    {
        SetText($"{equip.itemName} - {RandomNameGenerator.Capitalize(equip.equipmentType.ToString())} : {quantity}");
    }
    public virtual void NothingPrint()
    {
        SetText($"{PrintSlotName(slot)}: No equipment");
    }
    public virtual void LoadEquipment(Equipment equip, int quantity)
    {
        if(equip != null)
        {
            EquipmentPrint(equip, quantity);
            SetImageColor(BattleConstants.GetBadgeColor(equip.rarity));
            SetIcon(equip); 
        }
        else
        {
            NothingPrint();
            SetImageColor(BattleConstants.GetBadgeColor(RarityOptions.Common));
            SetIcon(null);
        }

        SetButtonInteractable(UI_FullArmyDisplay.CurrentHero != null);
    }
    public Equipment PullEquipment(Hero hero)
    {
        if (hero == null) return null;

        return slot switch
        {
            Enum_EquipmentSlot.weapon => hero.equipment.weapon,
            Enum_EquipmentSlot.armor => hero.equipment.armor,
            Enum_EquipmentSlot.accessoryA => hero.equipment.accessoryA,
            Enum_EquipmentSlot.accessoryB => hero.equipment.accessoryB, 
            // null
            _ => null,
        };
    }
    public static string PrintSlotName(Enum_EquipmentSlot slot)
    {
        return slot switch
        {
            Enum_EquipmentSlot.weapon => "Weapon",
            Enum_EquipmentSlot.armor => "Armor",
            Enum_EquipmentSlot.accessoryA => "Accessory A",
            Enum_EquipmentSlot.accessoryB => "Accesory B",
            Enum_EquipmentSlot.inventory => "",
            // weapon
            _ => "Weapon",
        };
    }
    public static string PrintTypeName(Enum_Equipment_Types type)
    {
        return RandomNameGenerator.Capitalize(type.ToString());
    }
    public void PassSelection()
    {
        LastSelection = slot;
        OnUpdatedSelection?.Invoke(LastSelection);
        Debug.Log($"Changing desired selection to: {LastSelection}");
    }
    public void ChangeEquipment()
    {
        if (lastSlot == null) return;

        Debug.Log($"{name} : Sending equipment change request = " +
            $"{(lastSlot != null && lastSlot.item ? lastSlot.item.name : "null")}");
        UI_FullArmyDisplay.ChangeEquipment(LastSelection, lastSlot.item as Equipment);
    } 
    public void SetButtonInteractable(bool setting)
    {
        if (!button) return;

        button.interactable = setting;
    }
    public void SetIcon(Item item)
    { 
        if (!icon) return;

        if (!item)
        {
            icon.color = Color.clear; 
        }

        else
        {
            icon.sprite = item.icon;
            icon.color = item.color; 
        }
    }

    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        // if (UI_FullArmyDisplay.CurrentHero == null) return;
        if (lastItem == null && lastSlot == null) return;

        Equipment i = lastItem;
        if (!i) i = lastSlot.item as Equipment;

        if (!i) return;
        else TooltipManager.OpenTooltip(i.PrintToolTip(true), transform.position + tooltipOffset);
    }

    public virtual void OnPointerExit(PointerEventData eventData)
    {
        TooltipManager.CloseTooltip();
    }
}
