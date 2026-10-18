using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UI_NewITemButton : ComponentLoader
{
    public TextMeshProUGUI description;
    public Image backing;
    public Item lastItem = null;

    public override void PassObject(object obj)
    {
        if (obj is Item item)
        {
            SetText(item.itemName);
            SetDescription(item.PrintShortTooltip());
            SetImageSprite(item.icon);
            SetImageColor(item.color);

            if (backing) backing.color = BattleConstants.GetBadgeColor(item.rarity);

            lastItem = item;
        } 
    }
    public void SetDescription(string d)
    {
        if (description) description.text = d;
    }
    public void AddItemToInventory()
    {
        if (lastItem == null) return;
        GameManager.Player.inventory.AddItem(lastItem);
    }
}
