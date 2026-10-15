using UnityEngine;

[System.Serializable]
public class Battle_End_Turn : State
{
    public float endDelay = 1f;
    private float clock;

    public override void Initialize(StateMachine load)
    {
        StateID = "turn_end";
        parent = load;
    }
    public override void Enter()
    {
        clock = Time.time;
    }

    public override void Exit()
    {

    }

    public override void Next()
    {
        if (AutoBattleManager.CheckWinner()) // a team has been defeated
            parent.ChangeState("battle_end");

        else if (AutoBattleManager.TurnOrder.Count > 0)
            parent.ChangeState("turn_start");

        else parent.ChangeState("round_end");
    }

    public override void Update()
    {
        if (AutoBattleManager.Stack.Count > 0)
            clock = Time.time;

        if (Time.time >= clock + endDelay)
            Next();
    }
}
