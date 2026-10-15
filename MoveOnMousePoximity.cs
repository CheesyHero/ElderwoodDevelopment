using UnityEngine;

public class MoveOnMousePoximity : MonoBehaviour
{
    public bool active;
    public float enterThreshhold = 100f;
    public float exitThreshhold = 200f;

    public Vector2 positionOffset = new(0f, 100f);
    private Vector2 originalPosition;
    private Vector2 velocity = Vector2.zero;

    // This is SmoothDamp smooth time, not movement speed.
    public float smoothTime = 0.15f;

    private RectTransform rectTransform;

    private void Start()
    {
        rectTransform = transform as RectTransform;
        originalPosition = transform.localPosition;
    }

    private void Update()
    {
        Vector2 currentPosition = rectTransform != null
            ? rectTransform.position
            : transform.position;

        float d = Vector2.Distance(Input.mousePosition, currentPosition);

        if (!active && d < enterThreshhold)
            active = true;
        else if (active && d > exitThreshhold)
            active = false;

        Vector2 target = active ? originalPosition + positionOffset : originalPosition;

        transform.localPosition = Vector2.SmoothDamp(
            transform.localPosition,
            target,
            ref velocity,
            smoothTime
        );
    }
}