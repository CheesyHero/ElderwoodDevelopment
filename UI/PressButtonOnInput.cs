using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Button))]
public class PressButtonOnInput : MonoBehaviour
{
    [Header("Input Settings")]
    public string inputID = "cancel";
    public bool lockOnDialogue = true;
    public bool requirePointerInGame = true;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    private void OnEnable()
    {
        GlobalInputManager.OnButtonEnter += HandleInput;
    }

    private void OnDisable()
    {
        GlobalInputManager.OnButtonEnter -= HandleInput;
    }

    private void HandleInput(string id)
    {
        if (!string.Equals(
            id,
            inputID,
            System.StringComparison.OrdinalIgnoreCase))
            return;

        if (!button || !button.IsActive() || !button.IsInteractable())
            return;

        if (lockOnDialogue && DialoguePrinter.IsActive)
            return;

        if (requirePointerInGame && !IsPointerInsideGame())
            return;

        button.onClick.Invoke();
    }

    private bool IsPointerInsideGame()
    {
        if (Mouse.current == null)
            return false;

        Vector2 position = Mouse.current.position.ReadValue();

        return position.x >= 0 &&
               position.y >= 0 &&
               position.x < Screen.width &&
               position.y < Screen.height;
    }
}