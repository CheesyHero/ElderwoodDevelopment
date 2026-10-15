using UnityEngine;

[System.Serializable]
public abstract class State
{
    public string StateID { get; protected set; }
    protected StateMachine parent; 
    public virtual void Initialize(StateMachine load)
    {
        StateID = "new state";
        parent = load;
    }
    public abstract void Update();
    public abstract void Enter();
    public abstract void Exit();
    public abstract void Next();
}
