using UnityEngine;

public class Battle_Start_Turn : State
{ 
    public override void Initialize(StateMachine load)
    {
        StateID = "turn_start";
        parent = load;
    }
    public override void Enter()
    {
        //Debug.Log($"{parent.name} entered {StateID} state");
        //AutoBattleManager.LaunchEvent(StateID);
        AutoBattleManager.SetNextTurn();
    }

    public override void Exit()
    {
        //Debug.Log($"{parent.name} exited {StateID} state");
    }

    public override void Next()
    {
        //Debug.Log($"{parent.name} has {StateID} state going to next...");
        parent.ChangeState("turn_play");
    }

    public override void Update()
    {
        // Do nothing and await completion from actor 
    }
}
