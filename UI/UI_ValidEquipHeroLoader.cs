using System.Collections.Generic;
using UnityEngine;

public class UI_ValidEquipHeroLoader : MonoBehaviour
{
    public UI_QuickEquip_Hero buttonPrefab; 
    private List<UI_QuickEquip_Hero> children = new();

    public void LoadHeroes(List<Hero> heroes)
    {
        if (heroes == null) return;

        // Create additional buttons if needed
        while (children.Count < heroes.Count)
        {
            children.Add(Instantiate(buttonPrefab, transform));
        }

        // Disable unused buttons, starting from the end
        for (int i = children.Count - 1; i >= heroes.Count; i--)
        {
            children[i].gameObject.SetActive(false);
        }

        // Enable and populate buttons
        for (int i = 0; i < heroes.Count; i++)
        {
            children[i].gameObject.SetActive(true);
            children[i].PassObject(heroes[i]);
        }
    }
}