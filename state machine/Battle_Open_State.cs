using UnityEngine;

[System.Serializable]
public class Battle_Open_State : State
{
    public float openTime = 1f;
    private float clock;

    public override void Initialize(StateMachine load)
    {
        StateID = "battle_open";
        parent = load;
    }

    public override void Enter()
    {
        //Debug.Log($"{parent.name} entered {StateID} state"); 
        //AutoBattleManager.LaunchEvent(StateID);
        clock = Time.time;

        ActiveMenu.CloseByID("Map Canvas");
        ActiveMenu.OpenByID("Battle Canvas"); 
    }

    public override void Exit()
    {
        //Debug.Log($"{parent.name} exited {StateID} state");
    }

    public override void Next()
    {
        //Debug.Log($"{parent.name} has {StateID} state going to next..."); 
        parent.ChangeState("battle_effects");
    }

    public override void Update()
    {
        if (Time.time >= clock + openTime)
            Next();
    }
}
