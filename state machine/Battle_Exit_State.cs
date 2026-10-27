using UnityEngine;

[System.Serializable]
public class Battle_Exit_State : State
{
    // This state is for pushing the animation events for ending battle
    // We want some new things
    // We want to add an animation, showing how much EXP the player has earned
    // We then want an animation showing which team is retreating
    // If it's a stronghold space, only the attacker retreats
    // first, add retreating to the Battle Units 

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

        // move these to a new state
        ////AutoBattleManager.PrepWinner(); 
        ////AutoBattleManager.GrantEXP();

        // changes canvases
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

        AutoBattleManager.CleanUp();
    }

    public override void Update()
    {
        if (Time.time > clock + exitTime) 
            Next();
    }

}
