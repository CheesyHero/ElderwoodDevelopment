using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WS_ModeArt_Child : MonoBehaviour
{
    [SerializeField]
    protected SelectionTileDisplay tileDisplay;
    private void Awake()
    {
        if(!tileDisplay) tileDisplay = GetComponentInChildren<SelectionTileDisplay>();
    }

    public void SetSelectionColor(E_WalkSpace_SelectionType type)
    {
        //Debug.Log(name + " attempting to set color to " + type);
        if (tileDisplay) tileDisplay.SetColorToSetting(type);
    }

    public void Open()
    {
        gameObject.SetActive(true);
    }
    public void Close()
    {
        gameObject.SetActive(false);
    }
}
