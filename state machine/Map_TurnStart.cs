using UnityEngine;

[System.Serializable]
public class Map_TurnStart : State
{ 

    public override void Initialize(StateMachine load)
    {
        StateID = "map_turn_start";
        parent = load; 
    }

    public override void Enter()
    {
        TurnManager.NextTurn();

        ActiveMenu.OpenByID("Map Canvas");

        EnemyArmyManager.CurrentTurnArmy().StartTurn();
    }

    public override void Exit()
    {
    }

    public override void Next()
    {
        parent.ChangeState("map_main_phase");
    }

    public override void Update()
    {
        Next();
    }

}
