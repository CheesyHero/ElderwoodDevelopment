using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SellItemMenu : MonoBehaviour
{
    public Image icon;
    public TextMeshProUGUI weaponSellHeaderText;
    public TMP_InputField itemSellAmountText;
    public TextMeshProUGUI totalSellValueText;

    public void LoadValues()
    {  
        bool contains = InventoryManager.picked.item != null;
        string itemName = contains ? InventoryManager.picked.item.itemName : "NULL";
        string plural = (contains ? InventoryManager.picked.amount : -1) >= 1 ? "s" : "";
        string sellAmount = InventoryManager.sellAmount.ToString();
        string sellValue = Mathf.CeilToInt(InventoryManager.sellAmount * (contains ? InventoryManager.picked.item.cost : 0) * 0.5f).ToString();

        icon.sprite = contains ? InventoryManager.picked.item.icon : null;
        icon.color = contains ? InventoryManager.picked.item.color : Color.clear; 

        weaponSellHeaderText.text = $"How many {itemName}{plural} do you want to sell?"; 
        itemSellAmountText.text = sellAmount;
        totalSellValueText.text = $"{sellValue}g";
    }
}
