using System.Collections.Generic;
using UnityEngine; 
using UnityEngine.Events; 

[System.Serializable]
public class Inventory
{
    [SerializeField]
    protected int money = 1000;
    [SerializeField]
    protected List<InventorySlot> ledger = new(); 
    [SerializeField]
    protected List<InventorySlot> newItems = new();
    public AudioClip[] onMoneyChangeSFX;

    public static UnityAction<Inventory> OnInventoryUpdated; 


    public InventorySlot[] GetLedger()
    {
        return ledger.ToArray();
    } 
    public void ClearInventory()
    { 
        ledger.Clear();

        SortLedger();
    } 
    public void LoadInventory(Item[] items, int[] quantities)
    {
        ledger = new(InventorySlot.CreateInventoryArray(items, quantities));

        SortLedger();
    } 
    public void AddItem(Item item, int quantity = 1)
    {
        if (item == null || quantity <= 0) return;

        InventorySlot next = new(item, quantity);
        AddItem(next);

        SortLedger();
    } 
    public void AddItem(InventorySlot next)
    {
        if (next == null) return;
         
        int i = IndexOfItem(next.item);
        if (i >= 0) ledger[i] += next;

        else ledger.Add(next);

        newItems.Add(next);

        SortLedger();
    } 
    public void RemoveItem(Item item, int quantity = 1)
    {
        int i = IndexOfItem(item);
        if (i >= 0) ledger[i] -= quantity;

        SortLedger();
    } 
    public void RemoveItem(string itemName, int quantity = 1)
    {
        Item item = null;
        foreach (var slot in ledger)
            if (slot.item.itemName == itemName)
                item = slot.item;

        if (item == null) return;

        int i = IndexOfItem(item);
        if (i >= 0) ledger[i] -= quantity;

        SortLedger();
    }

    public void RemoveItem(InventorySlot slot)
    {
        RemoveItem(slot.item, slot.amount);

        SortLedger();
    }
    public int FindQuanitity(string itemName)
    {
        foreach (var slot in ledger)
        {
            if (slot.item.itemName == itemName)
                return slot.amount;
        }

        return 0;
    }
    public bool ContainsItem(string itemName, int minimim = 1)
    {
        foreach (var slot in ledger)
        {
            if (slot.item.itemName == itemName)
                return slot >= minimim;
        }

        return false;
    }
    public bool ContainsItem(Item item, int minimim = 1)
    {
        foreach (var slot in ledger)
        {
            if (slot.item == item)
                return slot >= minimim;
        }

        return false;
    }
    public int IndexOfItem(Item item)
    {
        int i = 0;

        foreach(var slot in ledger)
        {
            if (slot.item == item)
                return i;

            i++;
        }

        return -1;
    } 

    public InventorySlot[] GetNewItems()
    {
        return newItems.ToArray();
    }

    public void ClearNewItems()
    {
        newItems.Clear();

        SortLedger();
    }
    public void CleanList()
    {
        List<int> empties = new();

        foreach (var item in ledger)
            if (item.item == null)
                empties.Add(ledger.IndexOf(item));

        foreach (var index in empties)
            ledger.RemoveAt(index);

        SortLedger();
    } 
     
    public int GetMoney()
    {
        return money;
    }
    public void GainMoney(int c)
    {
        money += c;

        PlayMoneySFX();
        OnInventoryUpdated?.Invoke(this);
    }
    public bool CanSpend(int c)
    {
        if (c < 0) return false;

        return c <= money;
    }
    public void SpendMoney(int c)
    {
        if (CanSpend(c))
            money = Mathf.Clamp(money - c, 0, 999999999);

        PlayMoneySFX();
        OnInventoryUpdated?.Invoke(this);
    }
    public void SetMoney(int set)
    {
        money = set;

        PlayMoneySFX();
        OnInventoryUpdated?.Invoke(this);
    }
    public void PlayMoneySFX()
    {
        if (onMoneyChangeSFX.Length <= 0) return;

        AudioInstancer.PlayAudioInstance(onMoneyChangeSFX[Random.Range(0, onMoneyChangeSFX.Length)]);
    }

    public void SellItem()
    {

    }
    public void SortLedger()
    {
        Dictionary<Item, int> combined = new();

        foreach (var slot in ledger)
        {
            if (slot == null || slot.item == null || slot.amount <= 0) continue;

            if (combined.ContainsKey(slot.item))
                combined[slot.item] += slot.amount;
            else
                combined.Add(slot.item, slot.amount);
        }

        ledger.Clear();

        foreach (var entry in combined)
        {
            if (entry.Value <= 0) continue;

            ledger.Add(new InventorySlot(entry.Key, entry.Value));
        }

        ledger.Sort((a, b) => string.Compare(
            a.item.name,
            b.item.name,
            System.StringComparison.OrdinalIgnoreCase
        ));

        OnInventoryUpdated?.Invoke(this);
    }
}

[System.Serializable]
public class InventorySlot
{
    public Item item;
    public int amount = 0;

    public InventorySlot(Item input, int quantity)
    {
        item = input;
        amount = quantity;
    }
    public static InventorySlot[] CreateInventoryArray(Item[] items, int[] quantities)
    {
        List<InventorySlot> list = new();

        for (int i = 0; i < items.Length; i++)
        {
            list.Add(new InventorySlot(items[i], quantities[i]));
        }

        return list.ToArray();
    }

    public static InventorySlot operator +(InventorySlot input1, int quantity)
    {
        Item i = input1.item;
        int q = Mathf.Clamp(input1.amount + quantity, 0, 999);
        return new InventorySlot(i, q);
    } 
    public static InventorySlot operator +(InventorySlot slot1, InventorySlot slot2)
    {
        if (slot1.item != slot2.item)
            return slot1;

        else return new(slot1.item, slot1.amount + slot2.amount);
    }

    public static InventorySlot operator -(InventorySlot input1, int quantity)
    {
        Item i = input1.item;
        int q = Mathf.Clamp(input1.amount - quantity, 0, 999);
        return new InventorySlot(i, q);
    }

    public static InventorySlot operator /(InventorySlot input1, int quantity)
    {
        Item i = input1.item;
        int q = Mathf.Clamp(input1.amount / quantity, 0, 999);
        return new InventorySlot(i, q);
    }

    public static InventorySlot operator *(InventorySlot input1, int quantity)
    {
        Item i = input1.item;
        int q = Mathf.Clamp(input1.amount * quantity, 0, 999);
        return new InventorySlot(i, q);
    }
    public override bool Equals(object obj)
    {
        return base.Equals(obj);
    }
    public override int GetHashCode() => base.GetHashCode();
    public static bool operator ==(InventorySlot input1, int total)
    {
        return input1.amount == total;
    }
    public static bool operator !=(InventorySlot input1, int total)
    {
        return input1.amount != total;
    }
    public static bool operator <(InventorySlot input1, int total)
    {
        return input1.amount < total;
    }
    public static bool operator >(InventorySlot input1, int total)
    {
        return input1.amount > total;
    }
    public static bool operator <=(InventorySlot input1, int total)
    {
        return input1.amount <= total;
    }
    public static bool operator >=(InventorySlot input1, int total)
    {
        return input1.amount >= total;
    }

}
