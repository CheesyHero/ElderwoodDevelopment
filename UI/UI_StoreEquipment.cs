using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using TMPro;

public class UI_StoreEquipment : UI_EquipmentButton, IPointerEnterHandler, IPointerExitHandler
{
    public static UnityAction OnPurchase;
    public TextMeshProUGUI costText; 

    [Header("Store Values")]
    public int storeIndex = 0;

    private void OnEnable()
    {
        LoadItem(); 
    }
    private void Awake()
    {
        OnPurchase += LoadItem;
    }
    private void OnDestroy()
    {
        OnPurchase -= LoadItem;
    }

    public void LoadItem()
    {
        PassObject(GameManager.GetStoreItem(storeIndex));
    }
    public override void PassObject(object obj)
    {
        base.PassObject(obj);

        Equipment equip = obj as Equipment;
        if (equip == null) return;

        int cost = lastItem ? lastItem.cost : -1;
        bool canSpend = GameManager.Inventory.CanSpend(cost);
        if (costText)
        {
            costText.text = $"{equip.rarity} { equip.equipmentType}\n" + (cost > 0 ? "Cost: " + cost.ToString("0g") : "--");
            costText.color = canSpend ? Color.white : Color.red;
        }

        SetButtonInteractable(canSpend);
    }
    public void BUTTON_TakeItem()
    {
        if (lastItem == null) return;

        Inventory inventory = GameManager.Inventory;
        if(inventory.CanSpend(lastItem.cost))
        {
            inventory.SpendMoney(lastItem.cost);
            inventory.AddItem(GameManager.CurrentEncounter.TakeStoreItem(storeIndex));
            PassObject(null);
            lastItem = null;
        }

        OnPurchase?.Invoke();
    }

    public override void EquipmentPrint(Equipment equip, int quantity)
    {
        if (equip == null) SetText("SOLD OUT");

        else SetText($"<u>{equip.itemName}</u>");
    }
    public override void NothingPrint()
    {
        SetText("<color=red>SOLD OUT</color>");
        if (costText)
        {
            costText.text = "--";
            costText.color = Color.gray;
        }
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        if (lastItem == null) return;
        else TooltipManager.OpenTooltip(lastItem.PrintToolTip(true), transform.position + tooltipOffset);
    }
}
