using UnityEngine;

public class Battle_Start_Round : State
{
    public override void Initialize(StateMachine load)
    {
        StateID = "round_start";
        parent = load;
    }
    public override void Enter()
    { 
        AutoBattleManager.SetInitiative();

        BattleCombatant.PlayGlobalEvent(Enum_BattleEvent_Type.onRoundStart);
        Next();
    }

    public override void Exit()
    {
        //Debug.Log($"{parent.name} exited {StateID} state");
    }

    public override void Next()
    {
        //Debug.Log($"{parent.name} has {StateID} state going to next...");
        if (AutoBattleManager.TurnOrder.Count > 0)
            parent.ChangeState("turn_start");

        else parent.ChangeState("battle_end");
    }

    public override void Update()
    {
    }
}
