using UnityEngine;

[System.Serializable]
public class Battle_Start_State : State
{
    public float startTime = 1f;
    private float clock;
    private bool limiter = false;

    public override void Initialize(StateMachine load)
    {
        StateID = "battle_start";
        parent = load;
    }

    public override void Enter()
    { 
        AutoBattleManager.StartBattle();  

        clock = Time.time;
        limiter = true;

        BattleCombatant.PlayGlobalEvent(Enum_BattleEvent_Type.onBattleStart);
    }

    public override void Exit()
    { 
    }

    public override void Next()
    {  
        parent.ChangeState("round_start");
        limiter = false;
    }

    public override void Update()
    {
        if (limiter && Time.time >= clock + startTime)
            Next();
    }
}
