using UnityEngine;

[System.Serializable]
public class Map_TurnMain : State
{ 

    public override void Initialize(StateMachine load)
    {
        StateID = "map_main_phase";
        parent = load;
    }

    public override void Enter()
    {
        ActiveMenu.OpenByID("Map Canvas");
    }

    public override void Exit()
    {
        //Debug.Log($"{parent.name} exited {StateID} state");
    }

    public override void Next()
    {
        parent.ChangeState("map_turn_end");
    }

    public override void Update()
    { 
    }

}
