using UnityEngine;

[System.Serializable]
public class Battle_Exit_State : State
{
    public float exitTime = 3f;
    private float clock;

    public override void Initialize(StateMachine load)
    {
        StateID = "battle_exit";
        parent = load;
    }

    public override void Enter()
    {
        //Debug.Log($"{parent.name} entered {StateID} state"); 
        //AutoBattleManager.LaunchEvent(StateID);
        clock = Time.time;

        AutoBattleManager.CleanUp();
        AutoBattleManager.PrepWinner();

        AutoBattleManager.GrantEXP();

        ActiveMenu.OpenByID("map canvas");
        ActiveMenu.CloseByID("battle canvas");

        HexWalker.RecieveBattleResults();
    }

    public override void Exit()
    {
        //Debug.Log($"{parent.name} exited {StateID} state");
        HexCrawlManager.CheckWinner();
    }

    public override void Next()
    {
        //Debug.Log($"{parent.name} has {StateID} state going to next..."); 
        parent.ChangeState("idle");
    }

    public override void Update()
    {
        if (Time.time > clock + exitTime) 
            Next();
    }

}
