using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_ShowOnPointerPosition : MonoBehaviour, IPointerExitHandler
{
    bool open = false;
    public float threshhold = 50f;
    public float openSpeed = 0.2f, closeSpeed = 0.05f;

    private void Start()
    {
        transform.localScale = Vector3.zero;
    }
    private void Update()
    {
        if (DistanceToPointer() < threshhold)
            Open();

        else Close();
    }

    public void Open()
    {
        if (open) return;

        LeanTween.cancel(gameObject);
        LeanTween.scale(gameObject, Vector3.one, openSpeed);

        open = true;
    }
    public void Close()
    {
        if (!open) return;

        LeanTween.cancel(gameObject);
        LeanTween.scale(gameObject, Vector3.zero, closeSpeed);

        open = false;
    }

    public float DistanceToPointer()
    {
        return Vector2.Distance(transform.position, Input.mousePosition);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Close();
    }
}
