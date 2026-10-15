using System;
using System.Collections.Generic;
using UnityEngine; 

public class GameManager : Army
{
    static GameManager manager;
    public static GameManager Player { get { return manager ? manager : null; } }
    public static int CurrentLevel { get { return Player.level; } }
    public static RunMapData CurrentMap { get { return Player.runMap; } }
    public static MapNodeData CurrentEncounter { get { return Player.lastNode; } }
    public static Inventory Inventory { get { return Player.inventory; } }
    public static int Distance { get { return (CurrentLevel * 5) + manager.lastNode.depth; } }

    [Header("Run Management")]
    public int level = 0;
    public World currentWorld; 
    public RunMapData runMap; 
    public MapNodeData lastNode; 

    protected override void Awake()
    {
        if (manager && manager != this)
            Destroy(gameObject);
        else
        {
            manager = this;
            DontDestroyOnLoad(gameObject);
            base.Awake();

            if (runMap.seed == 0 && currentWorld)
                SetNewWorld(currentWorld);
        } 
    } 

    public static EnemyProfile GetRandomOpponent()
    {
        if (Player.currentWorld == null) return null;

        else return Player.currentWorld.GetRandomProfile();
    } 
    public static void NextLevel()
    {
        if(Player.currentWorld == null) 
            return; 
        
        if(Player.currentWorld.nextWorld == null)
        {
            // PLAYER WINS!
            Debug.LogWarning("GAME OVER");
        }
        else
        {
            // Generate next level
            SetNewWorld(Player.currentWorld.nextWorld);
        }
    }
    public static void SetNewWorld(World next)
    {
        if (!Player) return;

        Player.currentWorld = next;
        Player.level = next.difficulty - 1;
        manager.runMap = RunMapGenerator.Generate(manager.currentWorld.mapSettings);
        manager.lastNode = manager.runMap.nodes[0];
    }
    public static void LoadNewEncounter(MapNodeData data)
    {
        if (!Player) return;

        Player.lastNode = data;
    }
    public static void CompleteCurrentLevel()
    {
        if (!Player) return;

        CurrentEncounter.completed = true;
    }
    public static Item GetStoreItem(int index)
    {
        return manager.lastNode.GetStoreItem(index);
    }
    public static Item TakeStoreItem(int index)
    {
        return manager.lastNode.TakeStoreItem(index);
    } 

    public static int GetPlayerSpawnCost(Unit unitIndex)
    {
        if(unitIndex == null) return 0;

        for (int i = 0; i < Player.units.Count; i++)
        {
            Unit item = Player.units[i];
            if (item == unitIndex)
                return GetPlayerSpawnCost(i);
        }

        return -1;
    }
    public static int GetPlayerSpawnCost(int unitIndex)
    { 
        return Player ? Player.SpawnCost(unitIndex) : -1;
    }

    public static void NewGame(ArmyGenerator newArmy, World newWorld)
    {
        if (!manager) return;

        Player.party.Clear();
        Player.units.Clear();
        Player.deck.Clear();

        Player.inventory.ClearInventory();
        Player.inventory.SetMoney(100);

        Player.generationList = newArmy;
        Player.GenerateArmy();
        Player.PopulateArmy();
        Player.deck.LoadDeck(Player);

        SetNewWorld(newWorld);

        SceneChanger.ChangeScenes(2);
    }
}
