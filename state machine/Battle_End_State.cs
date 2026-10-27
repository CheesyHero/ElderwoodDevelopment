using UnityEngine;

[System.Serializable]
public class Battle_End_State : State
{
    // this state is for end battle triggers

    public float endTime = 1f;
    private float clock;
    private bool limiter = false;

    public override void Initialize(StateMachine load)
    {
        StateID = "battle_end";
        parent = load;
    }

    public override void Enter()
    {
        AutoBattleManager.EndBattle();

        clock = Time.time;
        limiter = true;

        BattleCombatant.PlayGlobalEvent(Enum_BattleEvent_Type.onBattleEnd);
    }

    public override void Exit()
    {

    }

    public override void Next()
    {
        // battle is over, go to rewards before exit
        parent.ChangeState("battle_rewards");
        limiter = false;
    }

    public override void Update()
    {
        if (limiter && Time.time >= clock + endTime)
            Next();
    }

}
