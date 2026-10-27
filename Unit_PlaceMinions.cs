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
    public bool PlaceCombatantOnNextTile(BattleCombatant combatant)
    {
        if (!combatant) return false;

        BattleTile next = GetRandomAvailableTile();

        if (!next)
        {
            Debug.LogWarning("Minion Rack: No available slots.");
            return false;
        }

        // Replace a defeated minion only if its tile was selected.
        RemoveDefeatedMinion(next);

        // BattleTile handles parenting, placement, and visuals.
        if (!next.SetHero(combatant, combatant.parent.Attacker))
            return false;

        next.gameObject.SetActive(true);
        next.transform.localScale = Vector3.zero;

        LeanTween.scale(next.gameObject, Vector3.one * 0.8f, 0.6f)
            .setEaseOutBack();

        return true;
    }

    private BattleTile ReplaceDefeatedMinion()
    {
        if (children == null || children.Length == 0)
            RefreshTiles();

        foreach (BattleTile tile in children)
        {
            if (!tile) continue;

            BattleCombatant existing = GetCombatantOnTile(tile, true);

            if (!existing || existing.IsAlive)
                continue;

            // Remove from the parent unit's minion registry.
            if (existing.parent != null)
                existing.parent.minions.Remove(existing);

            // Deactivate immediately so it stops participating in combat.
            existing.gameObject.SetActive(false);

            // Destroy at the end of the frame.
            Destroy(existing.gameObject);

            return tile;
        }

        return null;
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

    public BattleTile GetRandomAvailableTile()
    {
        if (children == null || children.Length == 0)
            RefreshTiles();

        List<BattleTile> available = new();

        foreach (BattleTile tile in children)
        {
            if (!tile) continue;

            BattleCombatant occupant = GetCombatantOnTile(tile, true);

            // Living minions occupy their tiles.
            if (occupant != null && occupant.IsAlive)
                continue;

            // Empty tiles and defeated minions are both valid.
            available.Add(tile);
        }

        if (available.Count == 0)
            return null;

        int index = Random.Range(0, available.Count);
        return available[index];
    }
    private void RemoveDefeatedMinion(BattleTile tile)
    {
        if (!tile) return;

        BattleCombatant existing = GetCombatantOnTile(tile, true);

        if (!existing || existing.IsAlive)
            return;

        // Remove the old minion from its unit.
        if (existing.parent != null)
            existing.parent.minions.Remove(existing);

        // Detach immediately so the tile no longer contains the old minion.
        existing.transform.SetParent(null, true);

        // Deactivate and destroy.
        existing.gameObject.SetActive(false);
        Destroy(existing.gameObject);
    }
}