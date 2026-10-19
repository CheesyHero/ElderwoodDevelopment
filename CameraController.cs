using System.Linq; 
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraController : MonoBehaviour
{
    public Camera cam;
    public float zoomSpeed;
    [SerializeField] private float zoomSmoothTime = 0.1f;
    public Vector2 zoomClamp = new(60, 80); 
    private float targetFOV;
    private float zoomVelocity;
    float lastInputTime;
    public float sticky = 1f;
    public float walkSpeed = 5f;
    public Vector2 bounds = new(10f, 10f);
    public Vector3 desiredPos = new();
    public float acceleration = 0.1f;
    public float tracking = 0.5f;
    public float mouseEdgeRange = 0.95f;
    public float maxMouseInput = 1f;
    public static CameraController Main { get; private set; }
    public float snapSpeed = 0.8f;

    private void Awake()
    {
        Main = this;
    }

    private void Start()
    {
        if (cam) targetFOV = cam.fieldOfView;
    }

    private void Update()
    { 
        if (lastInputTime > 0)
            lastInputTime -= Time.deltaTime;

        Translate();
        BoundingBox();
        Track();
        Zoom();
    }
    private void Zoom()
    {
        if (!cam) return;

        float input = GlobalInputManager.Zoom.y; // use axis, not magnitude

        targetFOV -= input * zoomSpeed;
        targetFOV = Mathf.Clamp(targetFOV, zoomClamp.x, zoomClamp.y);

        cam.fieldOfView = Mathf.SmoothDamp(
            cam.fieldOfView,
            targetFOV,
            ref zoomVelocity,
            zoomSmoothTime
        );
    }

    private Vector3 v1;
    private void Track()
    {
        transform.position = Vector3.SmoothDamp(transform.position, desiredPos, ref v1, tracking);
    }
    public float DistanceToDesired()
    {
        return Vector3.Distance(transform.position, desiredPos);
    }
    public void Move(Vector3 point, bool forced)
    {
        if (lastInputTime > 0 && !forced) return;

        desiredPos = point;
    } 
    public static void MoveToPoint(Vector3 point, bool forced)
    {
        if (!Main) return;
        else Main.Move(point, forced);
    }
    public static void MoveToGameObject(GameObject obj, bool forced)
    {
        if (obj == null) return;
        else Main.Move(obj.transform.position, forced);
    }

    [SerializeField] private Vector3 input;
    private Vector3 momentum = new();
    private Vector3 v2 = new();
    protected void Translate()
    {
        if (walkSpeed <= 0) walkSpeed = 0.1f;

        // Prioritize keyboard/controller input
        Vector2 globalInput = GlobalInputManager.Move;

        if (globalInput.sqrMagnitude > 0.01f)
        {
            input = new Vector3(globalInput.x, 0, globalInput.y);
        }
        else
        {
            input = GetMouseToScreenInput();
        }

        input = Vector3.ClampMagnitude(input, 1f);

        // Smooth acceleration and deceleration
        momentum = Vector3.SmoothDamp(
            momentum,
            input,
            ref v2,
            acceleration
        );

        // Ignore movement when nearly stopped
        if (momentum.sqrMagnitude < 0.0001f && input.sqrMagnitude < 0.0001f)
            return;

        // Block automatic camera movement during manual input
        if (input.sqrMagnitude > 0.01f)
            lastInputTime = sticky;

        desiredPos += Time.deltaTime * walkSpeed * momentum;
    }
    public void BoundingBox()
    { 
        if (desiredPos.x > bounds.x)
            desiredPos -= Time.deltaTime * walkSpeed * Vector3.right; 
        if (desiredPos.x < -bounds.x)
            desiredPos += Time.deltaTime * walkSpeed * Vector3.right;

        if (desiredPos.z > bounds.y)
            desiredPos -= Time.deltaTime * walkSpeed * Vector3.forward; 
        if (desiredPos.z < -bounds.y)
            desiredPos += Time.deltaTime * walkSpeed * Vector3.forward;
    }
    public static void GoToClosestBase(Team t)
    {
        if (!Main) return;

        TeamBase closest = Main.FindClosestBase(t);
        if (closest) MoveToPoint(closest.transform.position, false);
    }

    public TeamBase FindClosestBase(Team t)
    {
        // Only bases owned by t
        TeamBase[] bases = TeamBase.bases
            .Where(b => b != null && b.owner == t)
            .ToArray();

        return (TeamBase)ReturnClosestToPoint(bases, transform.position);
    }

    public virtual Selectable ReturnClosestToPoint(Selectable[] list, Vector3 point)
    {
        Selectable find = null;
        float dist = float.PositiveInfinity;

        foreach (var item in list)
        {
            if (item == null) continue;

            float d = Vector3.Distance(point, item.transform.position); // FIX: measure to the item
            if (d < dist)
            {
                dist = d;
                find = item;
            }
        }

        return find;
    }

    private Vector3 GetMouseToScreenInput()
    {
        // Ignore input when game window is unfocused
        if (!Application.isFocused)
            return Vector3.zero;

        // Ignore input when mouse is over UI
        if (EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject())
            return Vector3.zero;

        Vector2 mousePos = Input.mousePosition;

        // Ignore input outside the game window
        if (mousePos.x < 0 || mousePos.x >= Screen.width ||
            mousePos.y < 0 || mousePos.y >= Screen.height)
            return Vector3.zero;

        Vector3 output = Vector3.zero;

        float x = (mousePos.x / Screen.width) * 2f - 1f;
        float y = (mousePos.y / Screen.height) * 2f - 1f;

        if (x <= -mouseEdgeRange)
            output.x = -Mathf.Lerp(0.1f, maxMouseInput, Mathf.InverseLerp(mouseEdgeRange, 1f, -x));
        else if (x >= mouseEdgeRange)
            output.x = Mathf.Lerp(0.1f, maxMouseInput, Mathf.InverseLerp(mouseEdgeRange, 1f, x));

        if (y <= -mouseEdgeRange)
            output.z = -Mathf.Lerp(0.1f, maxMouseInput, Mathf.InverseLerp(mouseEdgeRange, 1f, -y));
        else if (y >= mouseEdgeRange)
            output.z = Mathf.Lerp(0.1f, maxMouseInput, Mathf.InverseLerp(mouseEdgeRange, 1f, y));

        return output;
    }
}
