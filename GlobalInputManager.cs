using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class GlobalInputManager : MonoBehaviour
{
    public static GlobalInputManager Manager { get; private set; }

    private InputSystem_Actions controls;

    public static Vector2 Zoom => Manager ? Manager.zoom : Vector2.zero;
    [SerializeField] private Vector2 zoom;

    public static Vector2 Move => Manager ? Manager.move : Vector2.zero;
    [SerializeField] private Vector2 move;

    public static Vector2 Look => Manager ? Manager.look : Vector2.zero;
    [SerializeField] private Vector2 look;

    public static bool Select => Manager && Manager.select;
    [SerializeField] private bool select;

    public static bool Interact => Manager && Manager.interact;
    [SerializeField] private bool interact;

    public static bool Cancel => Manager && Manager.cancel;
    [SerializeField] private bool cancel;

    public static bool Menu => Manager && Manager.menu;
    [SerializeField] private bool menu;

    public static UnityAction<string> OnButtonEnter;

    private void Awake()
    {
        if (Manager != null && Manager != this)
        {
            Destroy(gameObject);
            return;
        }

        Manager = this;
        DontDestroyOnLoad(gameObject);

        controls = new InputSystem_Actions();

        controls.Player.Move.performed += OnMovePerformed;
        controls.Player.Move.canceled += OnMoveCanceled;

        controls.Player.Look.performed += OnLookPerformed;
        controls.Player.Look.canceled += OnLookCanceled;

        controls.Player.Zoom.performed += OnZoomPerformed;
        controls.Player.Zoom.canceled += OnZoomCanceled;

        controls.Player.Select.performed += OnSelectPerformed;
        controls.Player.Select.canceled += OnSelectCanceled;

        controls.Player.Interact.performed += OnInteractPerformed;
        controls.Player.Interact.canceled += OnInteractCanceled;

        controls.Player.Cancel.performed += OnCancelPerformed;
        controls.Player.Cancel.canceled += OnCancelCanceled;

        controls.Player.Menu.performed += OnMenuPerformed;
        controls.Player.Menu.canceled += OnMenuCanceled;
    }

    private void OnEnable()
    {
        controls?.Enable();
    }

    private void OnDisable()
    {
        controls?.Disable();
    }

    private void OnDestroy()
    {
        if (controls == null)
            return;

        controls.Player.Move.performed -= OnMovePerformed;
        controls.Player.Move.canceled -= OnMoveCanceled;

        controls.Player.Look.performed -= OnLookPerformed;
        controls.Player.Look.canceled -= OnLookCanceled;

        controls.Player.Zoom.performed -= OnZoomPerformed;
        controls.Player.Zoom.canceled -= OnZoomCanceled;

        controls.Player.Select.performed -= OnSelectPerformed;
        controls.Player.Select.canceled -= OnSelectCanceled;

        controls.Player.Interact.performed -= OnInteractPerformed;
        controls.Player.Interact.canceled -= OnInteractCanceled;

        controls.Player.Cancel.performed -= OnCancelPerformed;
        controls.Player.Cancel.canceled -= OnCancelCanceled;

        controls.Player.Menu.performed -= OnMenuPerformed;
        controls.Player.Menu.canceled -= OnMenuCanceled;
    }

    private void OnMovePerformed(InputAction.CallbackContext ctx)
    {
        move = ctx.ReadValue<Vector2>();
    }

    private void OnMoveCanceled(InputAction.CallbackContext ctx)
    {
        move = Vector2.zero;
    }

    private void OnLookPerformed(InputAction.CallbackContext ctx)
    {
        look = ctx.ReadValue<Vector2>();
    }

    private void OnLookCanceled(InputAction.CallbackContext ctx)
    {
        look = Vector2.zero;
    }

    private void OnZoomPerformed(InputAction.CallbackContext ctx)
    {
        zoom = ctx.ReadValue<Vector2>();
    }

    private void OnZoomCanceled(InputAction.CallbackContext ctx)
    {
        zoom = Vector2.zero;
    }

    private void OnSelectPerformed(InputAction.CallbackContext ctx)
    {
        select = true;
        OnButtonEnter?.Invoke("select");
    }

    private void OnSelectCanceled(InputAction.CallbackContext ctx)
    {
        select = false;
    }

    private void OnInteractPerformed(InputAction.CallbackContext ctx)
    {
        interact = true;
        OnButtonEnter?.Invoke("interact");
    }

    private void OnInteractCanceled(InputAction.CallbackContext ctx)
    {
        interact = false;
    }

    private void OnCancelPerformed(InputAction.CallbackContext ctx)
    {
        cancel = true;
        OnButtonEnter?.Invoke("cancel");
    }

    private void OnCancelCanceled(InputAction.CallbackContext ctx)
    {
        cancel = false;
    }

    private void OnMenuPerformed(InputAction.CallbackContext ctx)
    {
        menu = true;
        OnButtonEnter?.Invoke("menu");
    }

    private void OnMenuCanceled(InputAction.CallbackContext ctx)
    {
        menu = false;
    }

    private void Update()
    {
        if (select) select = false;
        if (cancel) cancel = false;
        if (interact) interact = false;
        if (menu) menu = false;
    }
}