using UnityEngine;

public class MoveIn : MonoBehaviour
{
    public float delay = 0f;
    public float scaleTime = 0.5f;
    public Vector3 startOffset = Vector3.one;
    public Vector3 endOffset = Vector3.zero;
    [SerializeField]
    [Space]
    private Vector3 origin;

    private void Awake()
    { 
        origin = transform.position;
    }
    private void OnEnable()
    {
        Animate();
    }

    public void Animate()
    {
        transform.position = origin + startOffset;
        LeanTween.move(gameObject, origin + endOffset, scaleTime).setEaseOutBack().setDelay(delay);
    }
}
