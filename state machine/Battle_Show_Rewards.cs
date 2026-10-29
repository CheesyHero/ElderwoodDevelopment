using UnityEngine;

[System.Serializable]
public class Battle_Show_Rewards : State
{  
    public float exitTime = 0.5f;
    private float clock;

    public override void Initialize(StateMachine load)
    {
        StateID = "battle_rewards";
        parent = load;
    }

    public override void Enter()
    {
        Debug.Log($"{parent.name} entered {StateID} state"); 

        //AutoBattleManager.LaunchEvent(StateID);
        clock = Time.time;
         
        AutoBattleManager.PrepWinner(); 
        AutoBattleManager.GrantEXP();
         
        BattleUnit.EndStage();
    }

    public override void Exit()
    {
        //Debug.Log($"{parent.name} exited {StateID} state");
        HexCrawlManager.CheckWinner();
    }

    public override void Next()
    {
        //Debug.Log($"{parent.name} has {StateID} state going to next..."); 
        parent.ChangeState("battle_exit");
    }

    public override void Update()
    {
        if (Time.time > clock + exitTime) 
            Next();
    }

}
