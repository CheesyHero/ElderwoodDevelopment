using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ScaleIn : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public bool onEnable = true;
    public bool onPointerEnter = false;
    public bool onPointerExit = false;
    [Header("Main")]
    public float delay = 0f;
    public float scaleTime = 0.5f;
    public Vector3 startScale = Vector3.zero;
    public Vector3 endScale = Vector3.one;

    private void OnEnable()
    {
        if(onEnable) Animate();
    }

    public void Animate()
    {
        LeanTween.cancel(gameObject);
        transform.localScale = startScale;
        LeanTween.scale(gameObject, endScale, scaleTime).setEaseOutBack().setDelay(delay);
    }

    public void Animate(bool withDelay)
    {
        LeanTween.cancel(gameObject);
        transform.localScale = startScale;
        LeanTween.scale(gameObject, endScale, scaleTime).setEaseOutBack().setDelay(withDelay ? delay : 0);
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        if(onPointerEnter) Animate();
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        if (onPointerExit) Animate();
    }
}
