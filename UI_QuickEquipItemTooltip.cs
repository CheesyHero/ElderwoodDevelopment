using TMPro;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_QuickEquipItemTooltip : ComponentLoader
{
    public Image preview_oldItem;
    public TextMeshProUGUI preview_oldName;

    public override void PassObject(object obj)
    {
        if (obj is Equipment equip)
        {
            SetImageSprite(equip.icon);
            SetImageColor(equip.color);
            SetText(equip.PrintToolTip(true));
        }
    }
    public void PassPreviewIn(Hero hero, Equipment equip)
    { 
        if(hero == null || equip == null)
        {
            preview_oldItem.color = Color.clear;
            preview_oldName.text = "No Item";
        }

        else
        {
            if (equip is Weapon)
                SetColor(hero.equipment.weapon);

            else if (equip is Armor)
                SetColor(hero.equipment.armor);

            else
            {
                Equipment accessoryA = hero.equipment.accessoryA;
                Equipment accessoryB = hero.equipment.accessoryB;

                if (accessoryA == null || accessoryB == null)
                {
                    preview_oldItem.color = Color.clear;
                    preview_oldName.text = "No Item";
                } 

                else if (accessoryA.rarity <= accessoryB.rarity)
                    SetColor(hero.equipment.accessoryA);

                else SetColor(hero.equipment.accessoryB);
            }
        }
    }

    private void SetColor(Equipment equip)
    {
        preview_oldItem.sprite = equip.icon;
        preview_oldItem.color = equip.color;
        preview_oldName.text = equip.itemName;
    }
}
