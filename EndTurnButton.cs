using UnityEngine;
using UnityEngine.UI;

public class EndTurnButton : ComponentLoader
{
    public Button button;

    private void Awake()
    {
        TurnManager.OnEventTrigger += CheckEnabled;
    }
    private void OnDestroy()
    {
        TurnManager.OnEventTrigger -= CheckEnabled;
    }

    private void CheckEnabled(string action)
    {
        if (button) button.interactable = 
                TurnManager.CanAct(PlayerSelection.Team);
    }
}
