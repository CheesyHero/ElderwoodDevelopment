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


        // start tutorial Hello World
        if (TurnManager.Round == 0)
            DialoguePrinter.StartDialogue("Hello World");

        else if (TurnManager.Round == 1)
            DialoguePrinter.StartDialogue("how to battle");

        else if (TurnManager.Round == 2)
            DialoguePrinter.StartDialogue("map rewards 1");
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
