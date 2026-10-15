using UnityEngine;

[System.Serializable]
public class Map_Introduction : State
{
    public int state = 2;
    public float showtime = 3f;
    float clock;

    public override void Initialize(StateMachine load)
    {
        StateID = "map_introduction";
        parent = load;
    }

    public override void Enter()
    {
        clock = Time.time;
    }

    public override void Exit()
    {
        //Debug.Log($"{parent.name} exited {StateID} state");
    }

    public override void Next()
    {
        TurnManager.FireEvent("map_next_intro");

        if (state == 0)
            parent.ChangeState("map_turn_start");

        else
        {
            state--;
            clock = Time.time;

            if (state == 1) 
                HexCrawlManager.WinCondition(); 

            else if(state == 0) 
                HexCrawlManager.LoseCondition(); 
        }
    }

    public override void Update()
    {
        if (state > 2 && Time.time >= clock + 0.01f)
            Next();
        else if (Time.time >= clock + showtime)
            Next();
    }

}
