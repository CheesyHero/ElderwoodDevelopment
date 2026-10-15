using System.Collections.Generic;
using UnityEngine;

public class Unit_PlaceMinions : MonoBehaviour
{
    [SerializeField] private float buffer = 1.5f;

    public BattleTile[] children;

    private void Awake()
    {
        RefreshTiles();
        HideEmptyTilesOnly();
    }

    public void RefreshTiles()
    {
        children = GetComponentsInChildren<BattleTile>(true);
    }

    // -------------------------
    // Core helpers
    // -------------------------
    private BattleCombatant GetCombatantOnTile(BattleTile tile, bool includeInactive = true)
    {
        if (!tile) return null;
        return tile.GetComponentInChildren<BattleCombatant>(includeInactive);
    }

    private bool TileHasAnyCombatant(BattleTile tile)
        => GetCombatantOnTile(tile, true) != null;

    private bool TileHasLivingCombatant(BattleTile tile)
    {
        var c = GetCombatantOnTile(tile, true);
        return c != null && c.IsAlive;
    }

    // -------------------------
    // Existing placement (safer)
    // -------------------------
    public void PlaceCombatantOnNextTile(BattleCombatant combatant)
    {
        if (!combatant) return;

        BattleTile next = GetNextInactiveEmptyTile();
        if (!next) return;

        combatant.transform.SetPositionAndRotation(next.transform.position, next.transform.rotation);
        combatant.transform.SetParent(next.transform, true);
        combatant.placement = next.GetUnitPosition();

        // Once occupied, ensure tile is visible
        next.gameObject.SetActive(true);
        next.transform.localScale = Vector3.zero;
        LeanTween.scale(next.gameObject, Vector3.one * 0.8f, 0.6f).setEaseOutBack();

        SpaceVisibleTilesEvenly();
    }

    public BattleTile GetNextInactiveEmptyTile()
    {
        if (children == null || children.Length == 0) RefreshTiles();

        foreach (var tile in children)
        {
            if (!tile) continue;

            // "Hidden" + "Empty" only
            if (tile.gameObject.activeSelf == false && TileHasAnyCombatant(tile) == false)
                return tile;
        }
        return null;
    }

    // -------------------------
    // 1) Clear / remove defeated heroes
    // -------------------------
    public void ClearDefeatedHeroes(bool hideEmptiedTiles = true)
    {
        if (children == null || children.Length == 0) RefreshTiles();

        foreach (var tile in children)
        {
            if (!tile) continue;

            var combatant = GetCombatantOnTile(tile, true);
            if (!combatant) continue;

            if (combatant.IsAlive == false)
            {
                Destroy(combatant.gameObject);

                if (hideEmptiedTiles)
                    tile.gameObject.SetActive(false);
            }
        }

        SpaceVisibleTilesEvenly();
    }

    // -------------------------
    // 2) Only hide tiles without heroes
    // -------------------------
    public void HideEmptyTilesOnly()
    {
        if (children == null || children.Length == 0) RefreshTiles();

        foreach (var tile in children)
        {
            if (!tile) continue;

            bool hasLivingHero = TileHasLivingCombatant(tile);
            tile.gameObject.SetActive(hasLivingHero); // show if occupied, hide if empty/defeated
        }

        SpaceVisibleTilesEvenly();
    }
    public void CleanAndHideTiles()
    {
        if (children == null || children.Length == 0) RefreshTiles();

        foreach (var tile in children)
        {
            if (!tile) continue;

            var hasLivingHero = GetCombatantOnTile(tile);
            if(hasLivingHero) Destroy(hasLivingHero.gameObject);
            tile.gameObject.SetActive(false);
        }

        SpaceVisibleTilesEvenly();
    }

    // -------------------------
    // 3) Spacing ONLY visible tiles, centered at local X = 0
    // -------------------------
    public void SpaceVisibleTilesEvenly()
    {
        if (children == null || children.Length == 0) RefreshTiles();

        List<Transform> visibleTiles = new();
        foreach (var tile in children)
        {
            if (!tile) continue;
            if (tile.gameObject.activeSelf)
                visibleTiles.Add(tile.transform);
        }

        int count = visibleTiles.Count;
        if (count == 0) return;

        float totalWidth = (count - 1) * buffer;
        float startX = -totalWidth * 0.5f;

        for (int i = 0; i < count; i++)
        {
            Transform t = visibleTiles[i];
            Vector3 localPos = t.localPosition;
            localPos.x = startX + (i * buffer);
            t.localPosition = localPos;
        }
    }
}