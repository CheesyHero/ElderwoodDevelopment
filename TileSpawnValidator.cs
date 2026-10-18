using System.Collections.Generic;
using UnityEngine;

public class TileSpawnValidator : MonoBehaviour
{
    [SerializeField] private HexTile parent;

    private static readonly List<TileSpawnValidator> children = new();

    private void Awake()
    {
        if (parent == null)
            parent = GetComponentInParent<HexTile>();

        Register();
        Hide();

        PlayerSelection.OnCancel += HandleCancel;
        EnemyArmyManager.OnUnitSpawned += HandleUnitSpawned;
    }

    private void OnEnable()
    {
        Register();
    }

    private void OnDisable()
    {
        Hide();
    }

    private void OnDestroy()
    {
        Unregister();

        PlayerSelection.OnCancel -= HandleCancel;
        EnemyArmyManager.OnUnitSpawned -= HandleUnitSpawned;
    }

    private void Register()
    {
        if (!children.Contains(this))
            children.Add(this);
    }

    private void Unregister()
    {
        children.Remove(this);
    }

    private void HandleCancel()
    {
        HideAll();
    }

    private void HandleUnitSpawned(object _)
    {
        HideAll();
    }

    public static void ShowValid(Army army)
    {
        CleanupNullChildren();

        if (army == null)
        {
            HideAll();
            return;
        }

        foreach (var validator in children)
        {
            if (validator != null)
                validator.Show(army);
        }
    }

    public static void HideAll()
    {
        CleanupNullChildren();

        foreach (var validator in children)
        {
            if (validator != null)
                validator.Hide();
        }
    }

    private static void CleanupNullChildren()
    {
        children.RemoveAll(item => item == null);
    }

    private void Show(Army army)
    {
        if (parent == null || army == null)
        {
            Hide();
            return;
        }

        bool valid =
            parent.CurrentUnit == null &&
            parent.owner == army.team &&
            parent.AdjacentToBase(army.team);

        gameObject.SetActive(valid);
    }

    private void Hide()
    {
        if (gameObject.activeSelf)
            gameObject.SetActive(false);
    }
}