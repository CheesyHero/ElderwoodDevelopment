using UnityEngine;

public class MoveOnMouseAxisThreshold : MonoBehaviour
{
    public bool active;

    public bool useX = false;
    public bool useY = true;

    public float xThreshold = 300f;
    public float yThreshold = 300f;

    public bool lessThan = true; // true = <= threshold, false = >= threshold

    public Vector2 positionOffset = new(0f, 100f);

    public bool affectX = true;
    public bool affectY = true;

    private Vector2 originalPosition;
    private Vector2 velocity = Vector2.zero;

    public float smoothTime = 0.15f;

    private RectTransform rectTransform;

    private void Start()
    {
        rectTransform = transform as RectTransform;
        originalPosition = transform.localPosition;
    }

    private void Update()
    {
        Vector2 mouse = Input.mousePosition;

        bool xCondition = !useX || (lessThan ? mouse.x <= xThreshold : mouse.x >= xThreshold);
        bool yCondition = !useY || (lessThan ? mouse.y <= yThreshold : mouse.y >= yThreshold);

        active = xCondition && yCondition;

        Vector2 baseTarget = active ? originalPosition + positionOffset : originalPosition;

        Vector2 current = transform.localPosition;

        Vector2 target = new Vector2(
            affectX ? baseTarget.x : current.x,
            affectY ? baseTarget.y : current.y
        );

        transform.localPosition = Vector2.SmoothDamp(
            current,
            target,
            ref velocity,
            smoothTime
        );
    }
}