using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;


public enum Enum_HexTile_Type
{
    plains = 0, road = 1, forest = 2, mountain = 3, water = 4, empty = 5,

    stronghold = 20, village = 21, wall = 22,
}
[SelectionBase]
public class HexTile : Selectable
{
    public int cost = 0;
    public static List<HexTile> all = new();
    public static HexTile[] AllTiles { get { return all.ToArray(); } }
    [Header("Tile Attributes")]
    public Enum_HexTile_Type type;
    public Team owner = new();
    private HexWalker current;
    public HexWalker CurrentUnit { get { return current; } }

    [SerializeField] protected Transform walkPoint;
    public Vector3 WalkPoint { get { return walkPoint ? walkPoint.position : transform.position; } }
    public List<HexTile> adjacents = new();

    public static UnityAction<HexTile> OnChangeOwnership; 
    public UnityAction<int> OnUpdateCost;
    public Sprite backdrop;

    public void Clear()
    {
        ChangeState(Enum_Selection_State.none);
    }
    
    public static void ResetAll()
    {
        foreach (var item in AllTiles)
            item.Clear();
    }
    private void Awake()
    {
        all.Add(this);
    }
    public virtual void SetInitialOwner()
    {
        if (owner.teamName.Length == 0)
            owner = new();
    }
    protected override void OnDestroy()
    {
        base.OnDestroy();
        all.Remove(this);
    }
    protected virtual void Start()
    {
        name = $"{type.ToSafeString()} ({transform.position.x:0.00}, {transform.position.z:0.00})";

        GetAdjacents();
        InitiateStartState();

        SetInitialOwner();
    }
    public virtual void InitiateStartState()
    {
        current = CheckForCurrent(); 
        if(current)
        {
            owner = current.Team;
            ChangeState(state);
            if ((int)type >= 20) // stronghold
                foreach(var tile in adjacents)
                {
                    tile.owner = owner;
                    tile.ChangeState(tile.state);
                }
        } 
    }
    public HexWalker CheckForCurrent()
    {
        // Radius should roughly match your tile footprint
        const float radius = 0.4f;

        Collider[] hits = Physics.OverlapSphere(transform.position, radius);

        foreach (var hit in hits)
        {
            HexWalker walker = hit.GetComponentInParent<HexWalker>();
            if (walker != null)
                return walker;
        }

        return null;
    }
    public virtual void Visit(HexWalker unit)
    {
        //Debug.Log($"{unit.name} is attempting to capture {this.name}");

        current = unit;
        Capture(unit, true);
    }
    public virtual void Visit(HexWalker unit, bool broadcast)
    {
        //Debug.Log($"{unit.name} is attempting to capture {this.name}");

        current = unit;
        Capture(unit, broadcast);
    }

    public virtual void Leave(HexWalker unit)
    {
        // Only the current occupant may clear the tile.
        if (unit == null) return;
        if (current != unit) return;

        current = null;
        ChangeState(Enum_Selection_State.none);
    } 
    //////////////////
    public override void OnHighlightEnter()
    {
        if (current)
            current.OnHighlightEnter();
        else
            base.OnHighlightEnter();
    }
    public override void OnHighlightExit()
    {
        if (current)
            current.OnHighlightExit();
        else
            base.OnHighlightExit();
    }
    public override void OnSelection()
    {
        if (current) 
            current.OnSelection();  
        else 
            base.OnSelection();  

    }
    public override void OnDeselect()
    {
        if (current)
            current.OnDeselect();
        else
            base.OnDeselect();
    }

    public void GetAdjacents()
    {
        adjacents.Clear();

        foreach(var item in Physics.OverlapSphere(transform.position, 1))
        {
            if(item != this && item.transform.TryGetComponent<HexTile>(out var hex)) 
                adjacents.Add(hex);
        }
        adjacents.Remove(this);
    }
    public HexTile GetClosestAdjacentInDirection(Vector3 point)
    { 
        HexTile closest = null;
        float distance = 99999;
        foreach(var tile in adjacents)
        {
            float d = Vector3.Distance(point, tile.transform.position);
            if(tile.current == null && d < distance)
            {
                closest = tile;
                distance = d;
            }
        }

        Debug.Log($"{name} : (Get Closest Search) searched {adjacents.Count} tiles and found : {closest}");
        return closest;
    }
    public HexTile GetClosestAdjacentInDirection(HexTile other)
    {
        if (!other) return null;
        else return GetClosestAdjacentInDirection(other.transform.position);
    }
    public virtual void Capture(Army army, bool broadcast)
    {
        if (owner == army.team) return;

        owner = army.team;

        if (!broadcast) return;

        if ((int)type >= 20)
            CurrentUnit.FatigueDecay();

        OnChangeOwnership?.Invoke(this); 
        HexCrawlManager.CheckWinner();
    }
    public void Capture(HexWalker unit, bool broadcast)
    {
        if(unit) Capture(unit.Army, broadcast);
    }
    public virtual bool AdjacentToBase(Team viewer)
    {
        if (this is TeamBase home && home.owner == viewer)
            return home;

        foreach (var item in adjacents)
            if (item is TeamBase && item.owner == viewer)
                return true;

        return false;
    } 
    public static void ResetCosts()
    {
        foreach (var item in AllTiles)
            item.UpdateCost(0);
    }
    public void ShowAllNearbyCosts(HexWalker unit)
    {
        ResetCosts();
        if(unit == null) return;

        //Debug.Log($"Showing hero unit {unit} with leader: {unit.Parent.Leader()}");
        foreach(var tile in adjacents)
            tile.CompareLeaderUnit(unit);
    }

    public void CompareLeaderUnit(HexWalker unit)
    {
        if(unit == null) UpdateCost(0);
        else
        {
            Hero leader = unit.Parent.Leader();
            LeaderTraits traits = leader.heroClass.leaderTraits; 
            int homePenalty = traits.ContainsUnfavoredType(unit.CurrentTile.type) ? 1 : 0;

            UpdateCost(1 + homePenalty);

            if (CurrentUnit != null && CurrentUnit.Team == unit.Team)
                UpdateCost(-1); // cant walk on allies

            else if (traits.ContainsNoWalkType(this.type))
                UpdateCost(-1); // cant walk on walls or pits unless flying

            else if (traits.ContainsUnfavoredType(this.type)) 
                UpdateCost(2 + homePenalty);

        }
    }

    public void UpdateCost(int i)
    {
        cost = i;
        OnUpdateCost?.Invoke(cost);
    }    

    public Color OwnerColor()
    {
        if (type != Enum_HexTile_Type.forest)
            return owner.teamColor;

        else return CurrentUnit && CurrentUnit.PlayerVisible() ? owner.teamColor : Color.grey / 2;
    }
}
