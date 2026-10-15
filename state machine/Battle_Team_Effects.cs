using UnityEngine;

[System.Serializable]
public class Battle_Team_Effects : State
{
    public float effectsTime = 1f;
    private float clock;

    public override void Initialize(StateMachine load)
    {
        StateID = "battle_effects";
        parent = load;
    }

    public override void Enter()
    {
        //Debug.Log($"{parent.name} entered {StateID} state"); 
        //AutoBattleManager.LaunchEvent(StateID);
        Debug.LogWarning("<color=red>ENTERING</color> : Team Effects");


        if (AutoBattleManager.ApplyTeamEffects())
        {
            Debug.Log("TEAM EFFECTS EXISTED");
            clock = Time.time;
        }

        else
        {
            Debug.Log("NO EFFECTS");
            Next();
        }
    }

    public override void Exit()
    {
        //Debug.Log($"{parent.name} exited {StateID} state");
    }

    public override void Next()
    {
        //Debug.Log($"{parent.name} has {StateID} state going to next..."); 
        parent.ChangeState("battle_start");
    }

    public override void Update()
    {
        if (Time.time >= clock + effectsTime)
            Next();
    }
}
