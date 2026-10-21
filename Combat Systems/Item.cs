using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum RarityOptions
{
    Common = 0, Uncommon = 1, Rare = 2, Mythic = 3, Legendary = 4
}
[CreateAssetMenu(fileName = "New Item", menuName = "NMD/RPG/Item", order = 2)]
public class Item : ScriptableObject
{
    public Sprite icon;
    public Color color = Color.white;
    [Header("Base Item Attributes")]
    public RarityOptions rarity;
    public string itemName = "new item";
    [TextArea(2, 8)]
    public string description = "a new item that does new things";
    public int cost = 100;

    public virtual string PrintToolTip(bool title)
    {
        return $"<u>{(title ? $"{itemName} : " : "")}{rarity}</u>\n{description}"; 
    }
    public override string ToString()
    {
        string hex = ColorUtility.ToHtmlStringRGB(color);
        return $"<color=#{hex}>{itemName} - {BattleConstants.WrapWithBadgeColor(rarity.ToString(), rarity)}</color>";
    }

    public virtual string PrintShortTooltip()
    {
        return description;
    }
}
