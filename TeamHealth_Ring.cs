using TMPro;
using UnityEngine;

public class TeamHealth_Ring : MonoBehaviour
{
    public TextMeshPro textMesh;
    public Gradient gradient;
    private HexWalker parent;
    private SpriteRenderer render;
    private void Awake()
    {
        parent = GetComponentInParent<HexWalker>();
        render = GetComponent<SpriteRenderer>();
    }
    private void Update()
    {
        SetValue();
        SetColor();
    }
    public void SetValue()
    {
        if(textMesh && parent)
        {
            textMesh.text = $"{Mathf.CeilToInt(parent.Parent.GetTotalHealthPercent() * 100)}%";
            textMesh.colorGradient = new()
            {
                topLeft = Color.white, 
                topRight = Color.white, 
                bottomLeft = gradient.Evaluate(parent.Parent.GetTotalHealthPercent()), 
                bottomRight = gradient.Evaluate(parent.Parent.GetTotalHealthPercent()),
            };
        }
    }
    public void SetColor()
    {
        if (render) 
            render.color = gradient.Evaluate(parent.Parent.GetTotalHealthPercent());
    }
}
