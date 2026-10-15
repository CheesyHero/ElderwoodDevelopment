using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine; 

public enum E_WalkSpace_SelectionType
{
    none = 0, highlighted = 1, selected = 2, blocked = 4
}
public enum E_WalkSpace_Mode
{
    basic = 0, water = 1, empty = 2, wall = 3, none = 4
}
[SelectionBase]
public class WalkSpace : MonoBehaviour
{
    readonly static List<WalkSpace> AllSpaces = new();

    [SerializeField]
    protected E_WalkSpace_SelectionType selectable;
    [SerializeField]
    protected E_WalkSpace_Mode mode;
    public int cost = 1;
    [SerializeField]
    protected WalkSpace[] adjacent = new WalkSpace[4]; 
    public WalkSpace[] GetAdjacent {  get { return adjacent; } }
    [SerializeField]
    protected WS_ModeArt_Parent art; 
    public Vector3 WalkPoint()
    {
        if (mode == E_WalkSpace_Mode.basic)
            return transform.position + Vector3.up * 0.5f;
        else return transform.position;
    }
    private void Awake()
    {
        art = GetComponentInChildren<WS_ModeArt_Parent>();

        FindAdjacents();
        SetCost();

        AllSpaces.Add(this);
    }
    private void OnDestroy()
    {
        AllSpaces.Remove(this);
    }
    public static void ResetAllSpaces()
    {
        foreach (var item in AllSpaces)
            item.Reset();
    }
    public void Reset()
    {
        SetSelectionType(E_WalkSpace_SelectionType.none);
    }

    public void CompleteArt()
    {
        if (!art) return; 
        art.UpdateTo(this);
    }
    public void SetSelectionType(E_WalkSpace_SelectionType type)
    {
        if (selectable == type) return;

        selectable = type;
        //Debug.Log(name + " attempting to set color to " + selectable);
        art.SetSelectionColor(selectable);
    }
    public E_WalkSpace_SelectionType GetSelectionType()
    {
        return selectable;
    }
    public void SetMode(E_WalkSpace_Mode setting)
    {
        mode = setting;
        SetCost();

        CompleteArt();
    }
    private void SetCost()
    {
        cost = mode switch
        {
            E_WalkSpace_Mode.basic => 1,
            E_WalkSpace_Mode.water => 2,
            E_WalkSpace_Mode.empty => 99,
            E_WalkSpace_Mode.wall => 99,
            E_WalkSpace_Mode.none => 99,
            _ => 99,
        };
    }
    public void SetMode(int input)
    {
        int highest = Enum.GetValues(typeof(E_WalkSpace_Mode)).Length - 1;
        if (input > highest)
            input = 0;
        else if(input < 0)
            input = highest;

        mode = (E_WalkSpace_Mode)input;
        CompleteArt();
    }

    public int GetModeIndex()
    {
        return (int)mode;
    }
    public E_WalkSpace_Mode GetSpaceMode()
    {
        return mode;
    } 

    public void FindAdjacents()
    {
        adjacent[0] = GetTileFromDir(Vector3.forward);
        adjacent[1] = GetTileFromDir(Vector3.right);
        adjacent[2] = GetTileFromDir(Vector3.back);
        adjacent[3] = GetTileFromDir(Vector3.left); 
    }
    public WalkSpace GetTileFromDir(Vector3 dir)
    {
        Vector3 halfEx = new(0.2f, 5f, 0.2f);
        Collider[] array = Physics.OverlapBox(transform.position + dir, halfEx);
        float highY = -9999;
        WalkSpace find = null;
        for (int i = 0; i < array.Length; i++)
        {
            if (array[i].TryGetComponent<WalkSpace>(out var space))
            {
                if (space == this) break;
                else if(space.transform.position.y > highY)
                {
                    highY = space.transform.position.y;
                    find = space;
                }
            }
        }

        return find;
    } 
    public WalkAgent GetCurrentAgent()
    { 
        Vector3 halfExtents = new(0.4f, 0.35f, 0.4f); 

        Collider[] hits = Physics.OverlapBox(transform.position + Vector3.up, halfExtents, Quaternion.identity, ~0, QueryTriggerInteraction.Collide);
        for (int i = 0; i < hits.Length; i++)
        {
            if (!hits[i]) continue;

            if (hits[i].TryGetComponent<WalkAgent>(out var agent))
                return agent;
            if (hits[i].transform.GetComponentInParent<WalkAgent>() is WalkAgent parentAgent)
                return parentAgent;
        }
        return null;
    }

}
