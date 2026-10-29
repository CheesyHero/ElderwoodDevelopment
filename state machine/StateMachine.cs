using UnityEngine;
using System.Collections.Generic;

public class StateMachine : MonoBehaviour
{
    public string lastIDPassed;
    protected State lastState;
    protected State currentState;
    public readonly List<State> states = new();

    public string GetCurrentStateID() => currentState?.StateID;

    protected virtual void Update()
    {
        currentState?.Update();
    }
    public void AddNewState(State state)
    {
        if(!states.Contains(state))
        {
            state.Initialize(this);
            states.Add(state);

            //Debug.Log($"{name}: Added new state {state.StateID}");
        }
    }
    public virtual void ChangeState(State state)
    {
        //if (lastState == state) return;

        lastState = currentState;

        currentState?.Exit();

        currentState = state;

        currentState?.Enter();

        lastIDPassed = currentState.StateID;
    }
    public virtual void ReturnToLast()
    {
        ChangeState(lastState);
    }
    public virtual bool ChangeState(string id_check)
    {
        //Debug.Log($"{name}: Searching for state with ID = {id_check}");
        if(currentState.StateID == id_check)
            return false;

        foreach (var item in states)
        {
            if(item.StateID.ToLower() == id_check.ToLower())
            {
                //Debug.Log($"{name}: SUCCESS = {item.StateID} has been found");
                ChangeState(item);
                return true;
            }
        }

        //Debug.Log($"{name}: FAILURE = {id_check} not found...");
        return false;
    }
    public virtual void Next()
    {
        currentState?.Next();
    }
}
