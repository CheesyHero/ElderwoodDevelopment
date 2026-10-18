using NUnit.Framework.Internal.Filters;
using UnityEngine;

public class FlyingLeaderHandle : MonoBehaviour
{
    public HexWalker parent;

    private bool setting;

    public Vector3 localStartPos = Vector3.zero;
    public float flyingSpeed = 1f;
    public float flyingScale = 1f;
    public Vector3 localFlyingOffset = Vector3.up;

    private float timeOffset = 0f;

    private void Awake()
    {
        if (!parent) 
            parent = GetComponentInParent<HexWalker>();
    }

    private void Start()
    {
        timeOffset = Random.Range(-1f, 1f);

        transform.localPosition = localStartPos;

        setting = Flying();
    }

    Vector3 flyingPos = new();
    private void Update()
    {
        if(setting != Flying())
            setting = Flying();

        if (setting == false) return;

        flyingPos.y = Mathf.Sin((Time.time + timeOffset) * flyingSpeed) * flyingScale;

        transform.localPosition = localFlyingOffset + flyingPos;
    }
    public bool Flying() 
    { 
        return parent ? parent.Flying : false; 
    }
}
