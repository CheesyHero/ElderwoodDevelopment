using System;
using UnityEngine;

public enum Enum_GameModes
{ 
    conquest, // capture your opponent's main base
    killOfTheHill, // Control various capture points until time is up
    raceToTheFinish, // Capture all designated bases before time is up
    capturePoints, // At the end of X turns, the player with the most bases wins
    elimination, // If a player controls no bases, they are eliminated
}
public class HexCrawlManager : MonoBehaviour
{
    public static GameMode CurrentGameMode { get; private set; }
    static HexCrawlManager manager;
    [Header("Game Mode")]
    public Enum_GameModes setting;

    [Tooltip("How many turns until the player loses the game.\n" + 
        "If the game is multiplayer, will result in the game ending with a tie breaker.")]
    public int turnTimer = -1;
    public static int TurnTimer { get { return manager ? manager.turnTimer : 0; } }
    public static int TurnsRemaining { get { return TurnTimer - TurnManager.Round; } }

    [Tooltip("Key bases assigned to the players entering the game.\n" +
        "For multiplayer maps, there must always be 4 bases.\n" +
        "If a base cannot be assigned to a player, that player will not be entered into the game.\n" +
        "If that Army is an AI army, an army without a base will be considered WILDERNESS instead.")]
    public TeamBase[] importantBases;
    public TeamBase[] capturePoints;
    public static TeamBase[] Bases
    { get { return manager ? manager.importantBases : new TeamBase[0]; } }
    public static TeamBase[] Captures
    { get { return manager ? manager.capturePoints : new TeamBase[0]; } }
    public static Army Winner { get; private set; }

    private void Awake()
    {
        manager = this; 
    }  
    private void Start()
    {
        // For DEBUG
        LoadGameMode(setting);
    } 
    public static void CheckWinner()
    {
        CurrentGameMode?.CheckGameOver();
    }
    public static void SetWinner(Army winner)
    {
        Winner = winner;
    }
    public static void LoadGameMode(Enum_GameModes desiredMode)
    {
        CurrentGameMode = CreateGameMode(desiredMode);
    }
    public static GameMode CreateGameMode(Enum_GameModes desiredMode)
    {
        return desiredMode switch
        {
            Enum_GameModes.conquest => new GameMode(),
            // Conquest
            _ => new GameMode(),
        };
    } 
    public static void WinCondition()
    {
        CurrentGameMode.PrintWinCondition(); 

        var hit = GetTeamBaseByIndex(1); 
        if (hit) CameraController.MoveToPoint(hit.WalkPoint, false);
    }
    public static void LoseCondition()
    {
        CurrentGameMode.PrintFailCondition();
        
        var hit = GetTeamBaseByIndex(0); 
        if (hit) CameraController.MoveToPoint(hit.WalkPoint, false);
    }
    public TeamBase GetTeamBaseByTeam(Team t)
    {
        foreach (var item in Bases)
            if (item.owner == t)
                return item;

        return null;
    }

    public static TeamBase GetTeamBaseByIndex(int i)
    {
        if (i >= 0 && i < Bases.Length)
            return Bases[i];

        return null;
    }
} 
