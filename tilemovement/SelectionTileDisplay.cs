using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionTileDisplay : MonoBehaviour
{
    [SerializeField]
    protected SpriteRenderer spriteRender;

    public Color noSelection = Color.clear;
    public Color highlightedColor = Color.cyan;
    public Color selectionColor = Color.yellow;
    public Color blockedColor = Color.grey;

    private void Awake()
    {
        spriteRender = GetComponent<SpriteRenderer>();
        SetColorToSetting(E_WalkSpace_SelectionType.none);
    } 

    public void SetColorToSetting(E_WalkSpace_SelectionType t)
    { 
        if(!spriteRender) return;

        spriteRender.color = GetColor(t);
        //Debug.LogWarning(name + " sprite should be result = " + GetColor(t));
    }

    public Color GetColor(E_WalkSpace_SelectionType t)
    { 
        return t switch
        {
            E_WalkSpace_SelectionType.highlighted => highlightedColor,
            E_WalkSpace_SelectionType.selected => selectionColor,
            E_WalkSpace_SelectionType.blocked => blockedColor,
            _ => noSelection,
        };
    }
}
