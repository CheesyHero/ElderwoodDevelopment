using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EnemyArmyManager : MonoBehaviour
{
    public HexWalker gamePiece;
    public static EnemyArmyManager manager; 
    public List<Army> armies = new();
    public static Army[] AllArmies { get { return manager.armies.ToArray(); } }
    public static int Combatants { get { return manager ? manager.armies.Count : 0; } }
    public static UnityAction<HexWalker> OnUnitSpawned; 

    private void Awake()
    {
        if (!manager) manager = this;
    }
    private void Start()
    {
        EnterLevel();
    }
    private void OnDestroy()
    {
        OnUnitSpawned = null;
    }
    public static void EnterLevel()
    {
        if (manager)
            manager.Initialize();
    }
    public void Initialize()
    {
        armies.Add(GameManager.Player);
        foreach (var army in GetComponentsInChildren<Army>())
            armies.Add(army);

        for(int  i = 0; i < armies.Count; i++)
        {
            var next = armies[i];
            next.knockOutsThisGame = 0;
            next.RefreshAll();
            next.homeBase = HexCrawlManager.Bases[i];
        }

        TeamBase.LoadAllTeams();
        HexWalker.InitAll();
    }
    public static int GetTeamIndex(Army army)
    {
        var all = AllArmies;
        for(int i = 0; i < AllArmies.Length; i ++)
        {
            var exists = all[i];
            if (exists && army == exists)
                return i;
        }

        return -1;
    }
    public static int GetTeamIndex(Team team)
    {
        var all = AllArmies;
        for(int i = 0; i < AllArmies.Length; i ++)
        {
            var exists = all[i];
            if (exists && team == exists.team)
                return i;
        }

        return -1;
    }

    private static HexWalker CreateInstance(Army army, int unit, HexTile location)
    {
        Debug.Log($"Creating new game piece from army index : {army} - unit index: {unit} - at tile {location.name}");
        if (!manager || manager.gamePiece == null) return null;
        if (CurrentTurnArmy() != army) return null;

        HexWalker instance = Instantiate(manager.gamePiece, location.WalkPoint, Quaternion.identity);
        instance.unitIndex = unit;
        instance.Initialize(army);

        instance.name = instance.Army.team.teamName + " " + instance.unitIndex.ToString("00");
        instance.Move(location);

        OnUnitSpawned?.Invoke(instance);
        CameraController.MoveToPoint(instance.transform.position, false);

        return instance;
    }
    public static HexWalker CheatGamePiece(Army army, int unit, HexTile location)
    {
        return CreateInstance(army, unit, location);
    }
    public static HexWalker CreateGamePiece(Army army, int unit, HexTile location)
    {
        int cost = army.SpawnCost(unit);

        if (cost < 0) return null;

        if(army.commandPoints >= cost)
        {
            army.UseCommandPoints(cost);
            return CreateInstance(army, unit, location);
        }
        else
        {
            Debug.Log($"<color=red>Not enough Command Points:</color> You need {cost} points.");
            return null;
        }
    } 

    public static void Defeat(Army loser)
    {
        loser.defeated = true;
        foreach(var item in HexWalker.activeWalkers)
        {
            if(item.Army == loser) 
                item.Defeat(null, true);
        }

        Debug.Log($"{loser.team} has lost the game");
    }
    public static List<Army> Remaining()
    {
        List<Army> list = new();
        foreach (var army in AllArmies)
            if (army.defeated == false) 
                list.Add(army);

        return list;
    }

    public static Army CurrentTurnArmy()
    {
        if (!manager || AllArmies.Length == 0) return null;

        try
        {
            return AllArmies[TurnManager.Turn];
        }
        catch
        {
            return null;
        }
    }

    public static int GetOtherKnockOuts(Army checker)
    {
        int total = 0;
        foreach (var army in AllArmies)
            if (army != checker)
                total += army.knockOutsThisGame;

        return total;
    }
}
