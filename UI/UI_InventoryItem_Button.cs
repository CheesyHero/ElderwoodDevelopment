using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_InventoryItem_Button : ComponentLoader, IPointerEnterHandler, IPointerExitHandler
{
    public Image rarityIcon;
    InventorySlot lastItem;

    public void OnPointerEnter(PointerEventData eventData)
    {
        ItemTooltipPrinter.LoadToolTip(lastItem.item);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        //Menu_PopUpCaller.CloseAll();
    }

    public void OpenMenu()
    {
        Menu_PopUpCaller.OpenMenu("InventoryMenu", transform.position);
        InventoryManager.SetNewSlot(lastItem);
    }

    public override void PassObject(object obj)
    {
        if (obj is InventorySlot item)
            LoadItem(item);
    }

    void LoadItem(InventorySlot slot)
    {
        SetImageSprite(slot.item.icon);
        SetImageColor(slot.item.color);
        SetText(slot.amount.ToString());

        lastItem = slot;

        if (rarityIcon)
            rarityIcon.color = BattleConstants.GetBadgeColor(slot.item.rarity);
    }
}
