using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UI_InventoryLoader_LayoutGroup : MonoBehaviour
{
    public UI_InventoryItem_Button buttonPrefab;

    public List<UI_InventoryItem_Button> children;

    private void OnEnable()
    {
        LoadInventory();
    }

    public void LoadInventory()
    {
        InventorySlot[] inventory = GameManager.Inventory.GetLedger();

        Debug.Log($"Inventory contains {inventory.Length} slots");
        Debug.Log($"Current UI children: {children.Count}");

        // Create additional buttons if needed
        while (children.Count < inventory.Length)
        {
            // Debug.Log("Creating inventory button");
            children.Add(Instantiate(buttonPrefab, transform));
        }

        // Disable unused buttons, starting from the end
        for (int i = children.Count - 1; i >= inventory.Length; i--)
        {
            children[i].gameObject.SetActive(false);
        }

        // Update and enable active inventory buttons
        for (int i = 0; i < inventory.Length; i++)
        {
            children[i].gameObject.SetActive(true);
            children[i].PassObject(inventory[i]);
        }
    }

}
