using System.Collections.Generic;
using UnityEngine; 

public class InventoryManager : MonoBehaviour
{
    public static InventorySlot picked;
    public static int sellAmount = -1;
    public static List<Hero> validHeroes = new();

    public static void SetNewSlot(InventorySlot slot)
    {
        picked = slot;
    }
     
    public static void OpenSellMenu()
    {
        if (picked == null) return;

        SetSellAmount(picked.amount);

        ActiveMenu.OpenByID("Sell Item");
    }

    public static List<Hero> FindAllValidHeroes()
    {
        if (picked == null || picked.item is not Equipment) return null;

        Equipment equipment = picked.item as Equipment;
        List<Hero> hits = new();
        foreach(var hero in GameManager.Player.party)
        {
            if (hero.heroClass.usableEquipment.Contains(equipment.equipmentType))
                hits.Add(hero);
        }

        return hits;
    }
    public static void SetSellAmount(string input)
    {
        if (int.TryParse(input, out int amount))
            SetSellAmount(amount);
    } 
    public static void SetSellAmount(int setting)
    {
        sellAmount = Mathf.Clamp(setting, 1, picked != null ? picked.amount : 0);
    }
    public static void AddSellAmount()
    {
        sellAmount = Mathf.Clamp(sellAmount + 1, 1, picked != null ? picked.amount : 0);
    }
    public static void RemoveSellAmount()
    {
        sellAmount = Mathf.Clamp(sellAmount - 1, 1, picked != null ? picked.amount : 0);
    }
    public static void Sell()
    {
        if (picked == null) return;

        int sale = Mathf.CeilToInt(picked.amount * picked.item.cost * 0.5f);
        // picked.amount -= sellAmount;

        GameManager.Inventory.RemoveItem(picked.item, sellAmount);
        GameManager.Inventory.GainMoney(sale); 
    }
}
