using UnityEngine;
using TMPro;

public class HT_WalkDistanceHelper : MonoBehaviour
{
    public HexTile parent;
    public GameObject handle;
    public TextMeshPro text;

    private void Awake()
    {
        if(parent == null) parent = GetComponentInParent<HexTile>();
        if (parent) parent.OnUpdateCost += SetCost;
    }
    private void Start()
    {
        SetCost(parent);
    }
    private void OnDestroy()
    {
        if (parent) parent.OnUpdateCost -= SetCost;
    }

    public void SetCost(int _) => SetCost(parent);
    public void SetCost(HexTile tile)
    {
        if(handle) handle.SetActive(tile.cost > 0);
        if (text) text.text = tile.cost.ToString();
    }

}
