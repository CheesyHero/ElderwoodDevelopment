using UnityEngine;

public class Battle_End_Round : State
{
    public override void Initialize(StateMachine load)
    {
        StateID = "round_end";
        parent = load;
    }
    public override void Enter()
    { 
        BattleCombatant.PlayGlobalEvent(Enum_BattleEvent_Type.onRoundEnd);
        Next();
    }

    public override void Exit()
    {

    }

    public override void Next()
    {
        //Debug.Log($"{parent.name} has {StateID} checking results. Total Remaining Actions: {BattleUnit.GetActivePoints()}");

        if (AutoBattleManager.CheckWinner()) // a team has been defeated
            parent.ChangeState("battle_end");

        else if (BattleUnit.GetActivePoints() > 0)
            parent.ChangeState("round_start");

        else parent.ChangeState("battle_end");
    }

    public override void Update()
    { 

    }
}
