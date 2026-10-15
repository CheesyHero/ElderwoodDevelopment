using UnityEngine;
using System.Collections.Generic;

public class EnableOnStateChangeCall : MonoBehaviour
{
    public bool initial;
    public GameObject handle;

    public List<string> enable = new();
    public List<string> disable = new();

    private void Awake()
    {
        TurnManager.OnEventTrigger += CheckState;
        AutoBattleManager.OnStateChange += CheckState;

        if (!handle) handle = gameObject;
    }
    private void OnDestroy()
    {
        TurnManager.OnEventTrigger -= CheckState;
        AutoBattleManager.OnStateChange -= CheckState;
    }
    private void Start()
    {
        gameObject.SetActive(initial);
    }
    public void CheckState(string state)
    {
        //Debug.Log($"{name} is checking for state change: {state} " +
        //    $"- With enable = {enable.Contains(state)} vs disable = {disable.Contains(state)}");

        if(enable.Contains(state))
            handle.SetActive(true);

        if(disable.Contains(state))
            handle.SetActive(false);
    }
}
