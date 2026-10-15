using UnityEngine;

public class BattleStateMachine : StateMachine
{ 
    protected Battle_Open_State battle_open;
    protected Battle_Start_State battle_start;
    protected Battle_End_State battle_end;
    protected Battle_Exit_State battle_exit;

    private void Awake()
    { 
        battle_open.Initialize(this);
        battle_start.Initialize(this);
        battle_end.Initialize(this);
        battle_exit.Initialize(this);
    }
    private void Start()
    {
        
    }
}
