using UnityEngine; 
using System.Collections;
using System.Collections.Generic; 

public class AITurnListener : MonoBehaviour
{
    [Header("AI Timing")]
    [SerializeField] private float actionDelay = 0.6f;
    private bool BattleLocked { get { return HexWalker.CurrentlyInBattle; } }
    private Coroutine aiRoutine;
    public static Army ActiveArmy { get; private set; }

    private void OnEnable()
    {
        TurnManager.OnEventTrigger += HandleTurnEvent; 
    }

    private void OnDisable()
    {
        TurnManager.OnEventTrigger -= HandleTurnEvent; 

        if (aiRoutine != null) { StopCoroutine(aiRoutine); aiRoutine = null; }
    } 

    private void HandleTurnEvent(string eventName)
    {
        // We only care when main phase starts
        if (eventName != "map_main_phase")
            return;

        int turnIndex = TurnManager.Turn;

        // Get the army for this turn
        if (EnemyArmyManager.CurrentTurnArmy().playerID == "AI")
            ActiveArmy = EnemyArmyManager.AllArmies[turnIndex];

        else ActiveArmy = null;

        if (ActiveArmy == null)
        {
            Debug.LogWarning($"AI Listener: No army found for turn index {turnIndex}");
            TurnManager.EndTurn(ActiveArmy);
            return;
        } 

        Debug.Log($"=== AI TURN START FOR ARMY [{ActiveArmy.team}] ===");
         
        List<AiUnitController> ownedAIUnits = new(); 
        foreach (var walker in HexWalker.activeWalkers)
        { 
            // must belong to this army
            if (walker.Army != ActiveArmy)
                continue;

            // must actually be an AI controller
            if (walker is AiUnitController aiCtrl && aiCtrl.moves > 0)
                ownedAIUnits.Add(aiCtrl);
        }

        Debug.Log($"AI Army {ActiveArmy.team} owns {ownedAIUnits.Count} AI-controlled walkers.");

        if (ownedAIUnits.Count == 0)
        {
            Debug.Log("No AI units with moves available, ending turn immediately.");
            TurnManager.EndTurn(ActiveArmy);
            return;
        }

        // If we somehow already had a routine running, stop it
        if (aiRoutine != null)  StopCoroutine(aiRoutine); 
        aiRoutine = StartCoroutine(AITurnRoutine(ownedAIUnits));
    }

    public bool PlacedNewUnit(List<AiUnitController> aiUnits)
    {
        bool success = false;
        if (CanPlaceNewUnits(ActiveArmy))
        {
            AiUnitController spawn = CreateNewUnit(ActiveArmy); 
            if (spawn != null)
            { 
                aiUnits.Add(spawn);
                success = true;
            }
        }

        return success;
    }
    public Enum_AI_Contoller_Modes GetRandomMode()
    { 
        return (Random.Range(0, 99) % 3) switch
        {
            1 => Enum_AI_Contoller_Modes.aggressive, // aggro units will prioritize enemy units before bases
            2 => Enum_AI_Contoller_Modes.defender, // defenders will find the nearest non-allied base and defend it
            //0                                    // if the only base unallied in the player base, will attempt to take it
            _ => Enum_AI_Contoller_Modes.march,    // Will move towards the player base. Will attack adjacent enemies.
        };
    }
    private IEnumerator AITurnRoutine(List<AiUnitController> aiUnits)
    { 
        yield return new WaitForSeconds(1);

        for(int i = 0; i < GameManager.CurrentLevel + 1; i++)
        {
            if (PlacedNewUnit(aiUnits))
                yield return new WaitForSeconds(0.6f);
        }

        yield return new WaitForSeconds(0.6f);

        foreach (AiUnitController actor in aiUnits)
        {
            if (!actor) continue;

            if (actor.controllerMode == Enum_AI_Contoller_Modes.idle)
                continue;
            else if (actor.controllerMode == Enum_AI_Contoller_Modes.defender && actor.CurrentTile is TeamBase)
                continue;

            while (actor && actor.moves > 0)
            {
                while (BattleLocked)
                    yield return null;

                actor.AI_PushMove();

                while (actor && actor.IsWalking && !BattleLocked)
                    yield return null;

                yield return new WaitForSeconds(actionDelay);
            }
        }

        while (BattleLocked)
            yield return null;

        aiRoutine = null;
        TurnManager.EndTurn(ActiveArmy);
    } 

    private AiUnitController CreateNewUnit(Army army)
    {
        if (army.defeated || army.playerID != "AI") return null;

        int selectedIndex = army.GetRandomAvailableUnit();
        HexTile tile = army.FindAvailableTile(army.units[selectedIndex]);

        if(selectedIndex >= 0 && tile != null)
        {
            HexWalker i = EnemyArmyManager.CheatGamePiece(army, selectedIndex, tile);
            if (i != null && i is AiUnitController unit)
                unit.GenerateMode();

            return i as AiUnitController;
        }
        else return null;
    }

    private bool CanPlaceNewUnits(Army army)
    { 
        if (army.defeated || army.playerID != "AI") return false;

        return army.HasReserveArmyReady();
    }
}
