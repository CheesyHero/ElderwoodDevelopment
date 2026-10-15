using UnityEngine;

[System.Serializable]
public class Map_TurnEnd : State
{ 

    public override void Initialize(StateMachine load)
    {
        StateID = "map_turn_end";
        parent = load;
    }

    public override void Enter()
    { 
    }

    public override void Exit()
    {
        //Debug.Log($"{parent.name} exited {StateID} state");
    }

    public override void Next()
    {
        parent.ChangeState("map_turn_start");
    }

    public override void Update()
    {
        Next();
    }

}
