using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class PlayerSelection : MonoBehaviour
{
    public static int Team => manager ? manager.teamIndex : 0;

    [SerializeField] private int teamIndex = 0;
    [SerializeField] ParticleSystem pointFeedback;
    [SerializeField] AudioClip selectionAudio;
    [SerializeField] AudioClip cancelAudio;

    private static PlayerSelection manager;

    [Header("Current State")]
    [SerializeField] private Selectable currentHighlight;
    [SerializeField] private Selectable currentSelection;

    public static Selectable GetCurrentHighlight => manager ? manager.currentHighlight : null;
    public static Selectable GetCurrentSelection => manager ? manager.currentSelection : null;

    public static UnityAction<Selectable> OnNewHighlight;
    public static UnityAction<Selectable> OnNewSelection;
    public static UnityAction OnCancel;

    public delegate void SelectionOverride(Selectable selection);
    public static SelectionOverride selectionOverride;

    private bool pointerOverUI;

    private void Awake()
    {
        if (manager != null && manager != this)
        {
            Debug.LogWarning($"Duplicate PlayerSelection found on {name}. Disabling duplicate.");
            enabled = false;
            return;
        }

        manager = this;
        selectionOverride = null;

        GlobalInputManager.OnButtonEnter -= HandleInput;
        GlobalInputManager.OnButtonEnter += HandleInput;
    }

    private void OnDisable()
    {
        if (manager != this)
            return;

        ClearHighlight();
        ClearSelection(false);
        TileSpawnValidator.HideAll();
    }

    private void OnDestroy()
    {
        if (manager != this)
            return;

        GlobalInputManager.OnButtonEnter -= HandleInput;

        ClearHighlight();
        ClearSelection(false);

        selectionOverride = null;
        manager = null;
    }

    private void Update()
    {
        pointerOverUI = QueryPointerOverUI();

        if (pointerOverUI)
        {
            ClearHighlight();
            return;
        }

        RefreshHighlight();
    }

    private void HandleInput(string id)
    {
        if (!enabled)
            return;

        if (string.IsNullOrWhiteSpace(id))
            return;

        id = id.Trim().ToLowerInvariant();

        // Cancel must always take priority.
        if (id == "cancel" || id == "menu")
        {
            CancelSelection(true);
            return;
        }

        if (pointerOverUI)
            return;

        switch (id)
        {
            case "select": 
                TrySelection();
                PlayPointFeedback();
                PlaySelectAudioFeedback();
                return;

            case "interact":
                TryInteract();
                return;
        }
    }

    private bool QueryPointerOverUI()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }

    private void RefreshHighlight()
    {
        SetHighlight(RaycastSelection());
    }

    private void SetHighlight(Selectable selection)
    {
        if (currentHighlight == selection)
            return;

        if (currentHighlight)
            currentHighlight.OnHighlightExit();

        currentHighlight = selection;

        if (currentHighlight)
            currentHighlight.OnHighlightEnter();

        OnNewHighlight?.Invoke(currentHighlight);
    }

    private void ClearHighlight()
    {
        SetHighlight(null);
    }

    public static void ForceSelection(Selectable selection)
    {
        if (!manager || selection == null)
            return;

        if (!manager.IsSelectableThisTurn(selection))
        {
            Debug.Log($"ForceSelection blocked: Not selectable this turn for team {manager.teamIndex}");
            return;
        }

        manager.SetHighlight(selection);
        manager.TrySelection();
    }

    private void TrySelection()
    {
        if (selectionOverride != null)
        {
            TryOverrideSelection();
            return;
        }

        SetSelection(currentHighlight);
    }

    private void TryOverrideSelection()
    {
        if (!IsOverrideTargetAllowed(currentHighlight))
        {
            Debug.Log(
                $"Override target blocked: Team {teamIndex} cannot use " +
                $"{currentHighlight?.name ?? "null"} as a target on turn {TurnManager.Turn}."
            );
            return;
        }

        selectionOverride?.Invoke(currentHighlight);
    }

    private void SetSelection(Selectable selection)
    {
        if (selection == null)
        {
            ClearSelection(true);
            return;
        }

        if (!IsSelectableThisTurn(selection))
        {
            Debug.Log($"Selection blocked: Team {teamIndex} cannot select {selection.name} on turn {TurnManager.Turn} (ownerTeam={selection.GetOwner()}).");
            return;
        }

        if (currentSelection == selection)
            return;

        if (currentSelection)
            currentSelection.OnDeselect();

        currentSelection = selection;

        if (currentSelection)
            currentSelection.OnSelection();

        OnNewSelection?.Invoke(currentSelection);
    }

    private void ClearSelection(bool invokeEvent)
    {
        if (currentSelection)
            currentSelection.OnDeselect();

        currentSelection = null;
        selectionOverride = null;

        TileSpawnValidator.HideAll();

        if (invokeEvent)
            OnNewSelection?.Invoke(null);
    }

    public static void Cancel()
    {
        if (!manager)
        {
            TileSpawnValidator.HideAll();
            OnCancel?.Invoke();
            return;
        }

        manager.CancelSelection(false);
    }

    private void CancelSelection(bool print)
    {
        PlayCancelAudioFeedback();

        ClearSelection(true);
        TileSpawnValidator.HideAll();

        OnCancel?.Invoke();

        if (print)
            BannerArray.PrintReport(" - Cancel Selection - ");
    }

    private void TryInteract()
    {
        if (currentSelection)
        {
            CancelSelection(false);
            return;
        }

        if (currentHighlight == null)
            return;

        if (!IsSelectableThisTurn(currentHighlight))
        {
            Debug.Log($"Interact blocked: Team {teamIndex} cannot interact with {currentHighlight.name} on turn {TurnManager.Turn}.");
            return;
        }

        SetSelection(currentHighlight);

        if (currentSelection != null)
            currentSelection.Interact();
    }

    private Selectable RaycastSelection()
    {
        Camera cam = CameraManager.CurrentCamera;
        if (!cam)
            return null;

        Vector3 mousePos = Input.mousePosition;

        if (!IsValidMousePosition(mousePos))
            return null;

        Ray ray = cam.ScreenPointToRay(mousePos);

        if (!Physics.Raycast(ray, out RaycastHit hit))
            return null;

        Debug.DrawRay(ray.origin, ray.direction * hit.distance, Color.red);

        return hit.transform.GetComponentInParent<Selectable>();
    }

    private bool IsValidMousePosition(Vector3 mousePos)
    {
        if (float.IsNaN(mousePos.x) || float.IsNaN(mousePos.y))
            return false;

        if (float.IsInfinity(mousePos.x) || float.IsInfinity(mousePos.y))
            return false;

        if (mousePos.x < 0 || mousePos.y < 0)
            return false;

        if (mousePos.x > Screen.width || mousePos.y > Screen.height)
            return false;

        return true;
    }

    private bool IsSelectableThisTurn(Selectable selectable)
    {
        if (selectable == null)
            return false;

        if (TurnManager.Turn != teamIndex)
            return false;

        return selectable.CanBeSelectedByTeam(teamIndex);
    }

    private bool IsOverrideTargetAllowed(Selectable selectable)
    {
        if (selectable == null)
            return false;

        if (TurnManager.Turn != teamIndex)
            return false;

        return true;
    }

    public void PlaySelectAudioFeedback()
    {
        AudioInstancer.PlayAudioInstance(selectionAudio);
    }
    public void PlayCancelAudioFeedback()
    {
        AudioInstancer.PlayAudioInstance(cancelAudio);
    }
    public void PlayPointFeedback()
    {
        if (!pointFeedback || Camera.main == null) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            pointFeedback.transform.position = hit.point;
            pointFeedback.Stop();
            pointFeedback.Play();
        }
    }
}