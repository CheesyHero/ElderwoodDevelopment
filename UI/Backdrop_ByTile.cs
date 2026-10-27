using UnityEngine;

public class Backdrop_ByTile : MonoBehaviour
{
    public SpriteRenderer backdrop;

    private void Awake()
    {
        if (!backdrop) backdrop = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        LoadBackdrop();
    }
    private void OnEnable()
    {
        LoadBackdrop();
    }

    private void LoadBackdrop()
    {
        if (HexWalker.DisputedTerritory == null || backdrop == null)
            return;

        if (HexWalker.DisputedTerritory.backdrop != null)
            backdrop.sprite = HexWalker.DisputedTerritory.backdrop;
    }
}
