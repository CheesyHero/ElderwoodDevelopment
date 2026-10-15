using UnityEngine;

[System.Serializable]
public class Battle_Idle : State
{ 

    public override void Initialize(StateMachine load)
    {
        StateID = "idle";
        parent = load;
    }

    public override void Enter()
    { 
    }

    public override void Exit()
    { 
    }

    public override void Next()
    { 
    }

    public override void Update()
    { 
    }

}
