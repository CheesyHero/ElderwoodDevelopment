using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum Enum_Selection_State
{
    none, highlight, selected, locked
}

[SelectionBase]
public abstract class Selectable : MonoBehaviour
{
    static readonly List<Selectable> all = new();
    public static Selectable[] AllSelectables { get { return all.ToArray(); } }

    // -1 = neutral / selectable by anyone; otherwise must match team index
    public Enum_Selection_State state = Enum_Selection_State.none;
    public static UnityAction<Selectable> OnStateChange;

    // helper check: is this selectable by a given team index?
    public virtual bool CanBeSelectedByTeam(int teamIndex)
    {
        // allow selection if neutral (-1) OR matches the team index
        return GetOwner() == -1 || GetOwner() == teamIndex;
    }
    protected virtual void OnEnable()
    {
        all.Add(this);
    }
    protected virtual void OnDisable()
    {
        all.Remove(this);
    }
    protected virtual void OnDestroy()
    {
        all.Remove(this);
    }
    public virtual int GetOwner()
    {
        return -1;
    } 
    public virtual void OnSelection()
    {
        ChangeState(Enum_Selection_State.selected);
    }
    public virtual void OnDeselect()
    {
        ChangeState(Enum_Selection_State.none);
    }
    public virtual void OnHighlightEnter()
    {
        if (state != Enum_Selection_State.selected)
            ChangeState(Enum_Selection_State.highlight);
    }
    public virtual void OnHighlightExit()
    {
        if (state != Enum_Selection_State.selected)
            ChangeState(Enum_Selection_State.none);
    }
    public virtual void Interact()
    {
        Debug.Log($"{name} : Interaction!");
    }
    public void ChangeState(Enum_Selection_State setting)
    {
        state = setting;
        OnStateChange?.Invoke(this);
    }
}
