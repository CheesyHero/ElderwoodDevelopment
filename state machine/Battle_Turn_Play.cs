using UnityEngine;

[System.Serializable]
public class Battle_Turn_Play : State
{ 

    public override void Initialize(StateMachine load)
    {
        StateID = "turn_play";
        parent = load;
    }

    public override void Enter()
    {
        AutoBattleManager.PushTurn();
        parent.ChangeState("turn_end");
    }

    public override void Exit()
    {
        //Debug.Log($"{parent.name} exited {StateID} state");
    }

    public override void Next()
    {
        //Debug.Log($"{parent.name} has {StateID} state going to next...");  
    }

    public override void Update()
    { 
    }

}
