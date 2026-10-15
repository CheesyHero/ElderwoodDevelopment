using System;
using UnityEngine;

public class TileRing_ColorComponent : MonoBehaviour
{
    public Color noHighlight = Color.gray;
    public Color yesHighlight = Color.yellow;
    public Color selection = Color.cyan;
    public Color locked = Color.black;

    private Selectable parent;
    private SpriteRenderer render;
    private void Awake()
    {
        parent = GetComponentInParent<Selectable>();
        render = GetComponent<SpriteRenderer>();

        Selectable.OnStateChange += SetColor;
        HexTile.OnChangeOwnership += UpdateColor;
        TurnManager.OnEventTrigger += CheckState;
    }
    private void Start()
    {
        SetColor(parent);
    } 
    private void OnDestroy()
    {
        Selectable.OnStateChange -= SetColor;
        HexTile.OnChangeOwnership -= UpdateColor;
        TurnManager.OnEventTrigger -= CheckState;
    } 
    private void UpdateColor(HexTile _)
    {
        if (parent is HexTile tile)
        {
            noHighlight = tile.OwnerColor();
            SetColor(parent);
        }
    }
    private void CheckState(string _)
    {
        if (parent is HexTile tile)
        {
            noHighlight = tile.OwnerColor();
            SetColor(parent);
        }
    } 
    public void SetColor(Selectable set)
    {
        if (set != parent) return;
        else if (render) 
            render.color = GetColor(set.state); 
    }
    public Color GetColor(Enum_Selection_State state)
    {
        return state switch
        {
            Enum_Selection_State.none => noHighlight,
            Enum_Selection_State.highlight => yesHighlight,
            Enum_Selection_State.selected => selection,
            Enum_Selection_State.locked => locked,
            _ => Color.white,
        };
    }
}
