using UnityEngine;

public class Battle_End_Round : State
{
    // this state is for END OF BATTLE triggers
    // Not actually the end of battle yet

    public override void Initialize(StateMachine load)
    {
        StateID = "round_end";
        parent = load;
    }
    public override void Enter()
    { 
        BattleCombatant.PlayGlobalEvent(Enum_BattleEvent_Type.onRoundEnd);
        Next();
    }

    public override void Exit()
    {

    }

    public override void Next()
    {
        int remainingAP = BattleUnit.GetActivePoints();
        bool defeated = AutoBattleManager.CheckWinner();

        Debug.LogWarning(
            $"END ROUND | AP: {remainingAP} | " +
            $"Defeated: {defeated} | " +
            $"TurnOrder: {AutoBattleManager.TurnOrder.Count}"
        );

        foreach (var hero in BattleCombatant.AllCombatants)
        {
            Debug.Log(
                $"{hero.name} | " +
                $"Alive: {hero.IsAlive} | " +
                $"AP: {hero.actionPoints}"
            );
        }

        if (defeated)
            parent.ChangeState("battle_end");
        else if (remainingAP > 0)
            parent.ChangeState("round_start");
        else
            parent.ChangeState("battle_end");
    }

    public override void Update()
    { 

    }
}
