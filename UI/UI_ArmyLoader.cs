using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;

public class UI_ArmyLoader : ComponentLoader
{
    protected Army parent;
    public UI_UnitButton[] units;
    public UnityAction<Unit> OnFetch;
    static int lastIndex = -1;
    private void Awake()
    {
        units = GetComponentsInChildren<UI_UnitButton>();
        foreach (var button in units)
            button.OnButtonPressed = OnButton;
    }
    private void OnEnable()
    {
        PassObject(GameManager.Player);
    }
    public void HideAll()
    {
        foreach (var button in units)
            button.SetActive(false);
    }
    public override void PassObject(object obj)
    {
        HideAll();
        if (obj is not Army army) return;

        parent = army;
        for(int i = 0; i < army.units.Count; i ++)
        {
            units[i].PassObject(army.units[i]);
            units[i].SetIndex(i);
        }
    }
    public void OnButton(int index)
    {
        if (parent == null) return;
        lastIndex = index;  

        var hit = HexWalker.Exists(parent, lastIndex); 
        if (hit != null)
        {
            Debug.Log($"{hit.name} was found with army = {parent.team} : and index = {lastIndex}");
            PlayerSelection.Cancel();
            PlayerSelection.ForceSelection(hit);
        }
        else
        { 
            Debug.Log($"{name} entering NEW ACTOR CREATION with army {parent.team} : with index = {lastIndex}");
            OnFetch?.Invoke(parent.units[lastIndex]);
            PlayerSelection.selectionOverride += PlaceNewUnit;
        }
    }

    private void PlaceNewUnit(Selectable selection)
    {
        PlayerSelection.selectionOverride -= PlaceNewUnit;

        if (!selection) return;

        Debug.Log($"Attempting to create new unit at {selection.name}...");

        if (selection is not HexTile tile) return;
        if (tile.CurrentUnit == true || !tile.AdjacentToBase(parent.team) || tile.owner != parent.team) return;

        EnemyArmyManager.CreateGamePiece(parent, lastIndex, tile);
    }

}
