using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events; 

public class AutoBattleManager : StateMachine
{
    static long _nextEventId = 1;
    public static AutoBattleManager Manager { get; private set; } 
    public static BattleEvent CurrentStack { get { return Stack.Count > 0 ? Stack[^1] : null; } } 
    public static readonly List<BattleEvent> Stack = new(); 
    public static List<BattleCombatant> TurnOrder = new(); 
    public static UnityAction<string> OnStateChange;
    public static UnityAction<BattleEvent> OnStackUpdated;
    public static UnityAction<BattleEvent> OnBattleEvent;
    public static UnityAction<Unit, Unit> OnResults;
    public static BattleCombatant currentTurn;

    [Header("Manager Tools")]
    [SerializeField] private BattleCombatant[] __view_turn_order__;
    public BattleUnit offense, defense;
    public static BattleUnit OffenseTeam { get { return Manager.offense; } }
    public static BattleUnit DefenseTeam { get { return Manager.defense; } }

    [Header("State Machine")]
    [SerializeField] protected Battle_Open_State battle_open = new();
    [SerializeField] protected Battle_Team_Effects battle_effects = new();
    [SerializeField] protected Battle_Start_State battle_start = new();
    [SerializeField] protected Battle_Start_Round round_start = new();
    [SerializeField] protected Battle_Start_Turn turn_start = new();
    [SerializeField] protected Battle_Turn_Play turn_play = new();
    [SerializeField] protected Battle_End_Turn turn_end = new(); 
    [SerializeField] protected Battle_End_Round round_end = new(); 
    [SerializeField] protected Battle_End_State battle_end = new();
    [SerializeField] protected Battle_Show_Rewards battle_rewards = new(); 
    [SerializeField] protected Battle_Exit_State battle_exit = new();
    [SerializeField] protected Battle_Idle idle = new();

    private void Awake()
    {
        if (Manager) Destroy(gameObject);
        else Manager = this;

        AddNewState(battle_open);
        AddNewState(battle_effects);
        AddNewState(battle_start);
        AddNewState(round_start);
        AddNewState(turn_start);
        AddNewState(turn_play);
        AddNewState(turn_end);
        AddNewState(round_end); 
        AddNewState(battle_end);
        AddNewState(battle_rewards); 
        AddNewState(battle_exit);
        AddNewState(idle);

        ChangeState(idle);
    }
    private void OnDestroy()
    {
        if (Manager != this) return;

        OnStateChange = null;
        OnBattleEvent = null;
        OnResults = null;
    }
    public static void CallEvent(BattleEvent action)
    {
        if (action == null) return;
         
        action.instanceId = _nextEventId++;

        OnBattleEvent?.Invoke(action);
    }

    public static void InitiateBattle(Unit attacker, Unit defender)
    {
        Debug.LogWarning($"Attempting to initiate battle: {attacker.team} -VS- {defender.team}");
        LaunchEvent("new_battle");

        _nextEventId = 1;

        if (!Manager) return;

        Manager.ChangeState(Manager.battle_open);
        Manager.LoadBattle(attacker, defender);

        Manager.ChangeState("battle_open");
    }
    public override bool ChangeState(string id_check)
    {
        // We don't want to change states while
        // unresolved triggers are on the stack

        if (Stack.Count > 0)
        {
            //Debug.LogWarning($"Attempting to Resolve Stack ({Stack.Count})");
            ResolveNext();
            return false;
        }
        else
        {
            //Debug.Log($"Stack is empty ({Stack.Count}), passing to next state ({id_check})..."); 
            base.ChangeState(id_check);
            LaunchEvent(currentState.StateID);

            return true;
        }
    }
    public static void AddToTopOfInitative(BattleCombatant actor)
    {
        TurnOrder.Remove(actor);
        TurnOrder.Insert(0, actor);
    }
    public static void SetInitiative()
    {
        TurnOrder = new();
        foreach (var item in BattleCombatant.AllCombatants)
            if (item.IsAlive && item.actionPoints > 0) 
                TurnOrder.Add(item);

        AdjustInitiative();
        View();
    } 
    public static void View()
    {
        Manager.__view_turn_order__ = TurnOrder.ToArray();
    }
    public static void AdjustInitiative()
    {
        TurnOrder.Sort((a, b) => b.Initiative().CompareTo(a.Initiative()));
    }
    public static void StartBattle()
    { 
        Debug.LogWarning("BATTLE STARTED");
    }
    public static void EndBattle()
    { 
        Debug.LogWarning("BATTLE ENDED");

        // Makes heroes leave accordingly
    } 
    public static void SetNextTurn()
    {
        int failsafe = 0; 
        PopNextTurn();
        while (currentTurn.hero.health.IsDead && failsafe < 20)
        {
            PopNextTurn();
            failsafe++;
        }

        Manager.Next();
    }
    private static void PopNextTurn()
    { 
        if(TurnOrder.Count > 0)
        {
            AdjustInitiative();
            currentTurn = TurnOrder[0]; 
            TurnOrder.RemoveAt(0);
            View();

            currentTurn.NotifyTurnStart();
        }
        else Manager.ChangeState("round_end");
    }
    public static void PushTurn()
    {
        if (currentTurn != null)
            currentTurn.TakeTurn();
    }
    public static void PushEndTurn()
    {
        if (currentTurn) currentTurn.NotifyTurnEnd();

        Manager.ChangeState("turn_end");
    }
    public static bool CheckWinner()
    {
        LaunchEvent("battle_check_win");
        return Manager.offense.IsDefeated() || Manager.defense.IsDefeated();
    } 
    public static void LaunchEvent(string event_id)
    { 
        OnStateChange?.Invoke(event_id);
    }
    public void LoadBattle(Unit attacker, Unit defender)
    {
        offense.parent = attacker; 
        offense.LoadUnits();

        defense.parent = defender; 
        defense.LoadUnits(); 
    }

    public static void ResolveNext()
    {
        if (Stack.Count == 0)
            return;

        var pull = Stack[^1];
         
        pull.Push(); 
    }
    public static void CompleteAction(BattleEvent completed, bool silent = false)
    {
        if (completed == null) return;
         
        Stack.Remove(completed);

        if (!silent)
            foreach (var t in completed.targets)
                t.AsOwner(completed, Enum_BattleEvent_Type.onAfterTargeted);

        if (Stack.Count > 0)
            ResolveNext();
        else
            PushEndTurn();

        StackUpdatedCall();
    }
    public static void AddToStack(BattleEvent action)
    {
        if (action == null) return;
        foreach (var item in Stack)
        {
            if (action.owner == item.owner && action.move == item.move)
                return; // checks the stack to see if we already made a reaction

            if (action.move.limited && action.move == item.move && action.owner.team == item.owner.team)
                return; // only one allied hero may have a limited move on the stack at a time 
        }
         
        Stack.Add(action);
        StackUpdatedCall();

        //Debug.Log($"<color=green>ADDING STACK</color> ({Stack.Count}): (<color=white>{action.eventType}</color>) : " +
        //    $"{action.owner} : {action.source} w/ <color=white>{action.DesiredMove}</color> : Target(s) = " +
        //    $"{(action.targets.Count == 1 ? action.targets[0] : action.targets.Count)} : ");

        CallEvent(action);

        foreach (var t in action.targets)
            t.AsOwner(action, Enum_BattleEvent_Type.onBeingTargeted);
    }
    public static void RemoveFromStack(BattleEvent action)
    {
        Stack.Remove(action);

        StackUpdatedCall();
    }
    private static void StackUpdatedCall()
    { 
        if (Stack.Count > 0) OnStackUpdated?.Invoke(Stack[^1]);
        else OnStackUpdated.Invoke(null);
    }
    public static void CleanUp()
    {
        Manager.offense.CleanUp();
        Manager.defense.CleanUp();
    }
    public static Unit winner = null, loser = null;
    public static void PrepWinner()
    {
        if (Manager.offense.workPoints < Manager.defense.workPoints)
        {
            winner = Manager.offense.parent;
            loser = Manager.defense.parent;
        }

        else
        {
            loser = Manager.offense.parent;
            winner = Manager.defense.parent;
        }
    } 
    public static void GrantEXP()
    {
        if (winner == null || loser == null) return;

        int winnerOutRewards = winner.GetTotalEXP() / 2;
        int loserOutRewards = loser.IsAlive() ? loser.GetTotalEXP() : loser.GetTotalEXP() * 2;

        winner.GrantEXP(loserOutRewards);
        loser.GrantEXP(winnerOutRewards); 
    } 
    public static void PrintPartyEXP(string print, Color color, Unit unit)
    {
        if (Manager.offense.parent == unit)
            PrintForEachHero(print, color, Manager.offense);

        else PrintForEachHero(print, color, Manager.defense); 
    }
    public static void PrintForEachHero(string print, Color color, BattleUnit unit)
    {
        foreach (var hero in unit.children)
            hero.PrintPopUp(print, color, 32);
    }

    public static bool StackContainsAsOwner(BattleCombatant hero)
    {
        foreach (var item in Stack)
            if (item.owner == hero)
                return true;

        return false;
    }

    public static bool ApplyTeamEffects()
    {
        Debug.LogWarning($"BATTLE MANAGER : Parsing all Team Effects");

        if (OffenseTeam.parent.Effects.Length > 0 || DefenseTeam.parent.Effects.Length > 0)
        {
            OffenseTeam.ApplyTeamEffects();
            DefenseTeam.ApplyTeamEffects();

            return true;
        }

        else return false;
    }
}
