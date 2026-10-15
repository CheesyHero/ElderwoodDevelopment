using System;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.Events;

public class TurnManager : StateMachine
{
    public static int Round { get; private set; }
    public static int Turn { get { return turnIndex; } }
    private static int turnIndex = -1;
    static TurnManager instance;
    public static UnityAction<string> OnEventTrigger;

    [SerializeField] protected Map_Introduction introduction = new();
    [SerializeField] protected Map_TurnStart turnStart = new();
    [SerializeField] protected Map_TurnMain mainPhase = new();
    [SerializeField] protected Map_TurnEnd turnEnd = new();
    [SerializeField] protected Map_GameOver gameOver = new();

    private void Awake()
    {
        instance = this;

        Round = 0;
        turnIndex = -1;

        AddNewState(introduction);
        AddNewState(turnStart);
        AddNewState(mainPhase);
        AddNewState(turnEnd);
        AddNewState(gameOver);
    }
    private void Start()
    {
        ChangeState(introduction);
    }
    public static void NextTurn()
    {
        if (HexWalker.BattleInProgress)
            return;

        TickTurn();
        Army currentTurn = EnemyArmyManager.AllArmies[turnIndex];
        if (currentTurn == null) //skip until next
        {
            int failsafe = 0;
            while((currentTurn == null || currentTurn.defeated) && failsafe < 10)
            {
                TickTurn();
                currentTurn = EnemyArmyManager.AllArmies[turnIndex];
                failsafe++;
            }
        }

        if (currentTurn)
        {
            BannerArray.PrintReport($"{currentTurn.team} : Turn Start! (round {Round})");
            CameraController.MoveToPoint(currentTurn.GetTurnPoint(), false);
        }
    }
    static void TickTurn()
    {
        turnIndex++;
        if (turnIndex >= EnemyArmyManager.Combatants)
        {
            turnIndex = 0;
            Round++;
        }

        if (Round >= HexCrawlManager.TurnTimer)
            EndGame();
    }
    public static void FireEvent(string action)
    {
        OnEventTrigger?.Invoke(action);
        Debug.Log("Turn Manager: Change state = " + action);
    } 
    public override void ChangeState(State state)
    { 
        base.ChangeState(state);
        PlayerSelection.Cancel();
        FireEvent(state.StateID);
        HexTile.ResetAll();
    }
    public static bool CanAct(int teamRequest)
    {
        return teamRequest == turnIndex;
    }
    public static void EndTurn(Army currentArmy)
    {
        if(currentArmy != EnemyArmyManager.CurrentTurnArmy())
                return;

        if (!instance || instance.currentState == null) return;

        if (instance.currentState.StateID == "map_main_phase")
            instance.Next();

        HexCrawlManager.CheckWinner();
    }
    public static void EndGame()
    {
        Debug.LogWarning("GAME ENDING");

        if (!instance || instance.currentState == null) return;
        instance.ChangeState("map_game_over");
    }

    public static bool IsTurn(GameManager check)
    {
        return EnemyArmyManager.CurrentTurnArmy() == check;
    }
}
