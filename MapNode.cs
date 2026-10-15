using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class MapNodeData
{
    public int id;
    public int depth;
    public Vector2 position;
    public MapNodeType type;
    public ArmyGenerator main; 
    public int sceneIndex;
    public bool completed = false;
    public List<int> nextNodeIds = new();
    public List<Item> storeInventory = new();
    public List<CommandCard> cardMarket = new();
    public List<HeroClass> recruits = new();

    public Item GetStoreItem(int index)
    {
        if (index < 0 || index >= storeInventory.Count)
            return null;

        return storeInventory[index];
    }
    public Item TakeStoreItem(int index)
    {
        Item i = GetStoreItem(index);

        if (i == null) return null;
        else
        {
            storeInventory[index] = null;
            return i;
        }
    }
}

public class MapNode : MonoBehaviour
{
    public MapNodeData data;
    public Image armyPreview;
    public Button button;

    private void Start()
    {
        if (armyPreview) armyPreview.color =
                data.completed ? GameManager.Player.team.teamColor : data.main.team.teamColor;
    }
    private void Update()
    {
        if (button && GameManager.Player)
            button.interactable = GameManager.Player.lastNode.nextNodeIds.Contains(data.id);
    }
    public Item GetStoreItem(int index)
    {
        return data.GetStoreItem(index);
    }
    public Item TakeStoreItem(int index)
    {
        return data.TakeStoreItem(index);
    }
    public void BUTTON_ClickMap()
    {
        if (data.completed) return;

        GameManager.LoadNewEncounter(data); 
        SceneChanger.ChangeScenes(data.sceneIndex);
    }
}
