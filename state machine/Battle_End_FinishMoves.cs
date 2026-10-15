using UnityEngine;

[System.Serializable]
public class Battle_End_FinishMoves : State
{
    public float endTime = 1f; 

    public override void Initialize(StateMachine load)
    {
        StateID = "battle_completing";
        parent = load;
    }

    public override void Enter()
    {
        Next();
    }

    public override void Exit()
    {

    }

    public override void Next()
    {
        parent.ChangeState("battle_exit"); 
    }

    public override void Update()
    { 
    }

}
