using UnityEngine;

[System.Serializable]
public class Map_GameOver : State
{ 

    public override void Initialize(StateMachine load)
    {
        StateID = "map_game_over";
        parent = load;
    }

    public override void Enter()
    {
        ActiveMenu.OpenByID("Results Screen");
        ActiveMenu.CloseByID("map convas");
    }

    public override void Exit()
    {
        //Debug.Log($"{parent.name} exited {StateID} state");
    }

    public override void Next()
    {
        // EXIT GAME
    }

    public override void Update()
    { 
    }

}
