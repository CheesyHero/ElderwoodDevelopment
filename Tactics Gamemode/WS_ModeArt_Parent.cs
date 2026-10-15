using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WS_ModeArt_Parent : MonoBehaviour
{
    [SerializeField]
    protected int index = 0;
    public WS_ModeArt_Child[] children;

    private void Start()
    {
        UpdateTo(GetComponentInParent<WalkSpace>());
    }
    public void SetSelectionColor(E_WalkSpace_SelectionType type)
    {
        //Debug.Log(name + " attempting to set color to " + type);
        children[index].SetSelectionColor(type);
    }
    public void UpdateTo(WalkSpace from)
    {
        if (from == null) return;

        HideAll();
        index = (int)from.GetModeIndex();
        children[index].Open();
    }
    public void HideAll()
    {
        foreach (var item in children)
            item.Close();
    }
}
