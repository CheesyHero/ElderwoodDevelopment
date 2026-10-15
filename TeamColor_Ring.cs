using UnityEngine;

public class TeamColor_Ring : MonoBehaviour
{
    private HexWalker parent;
    private SpriteRenderer render;
    private void Awake()
    {
        parent = GetComponentInParent<HexWalker>();
        render = GetComponent<SpriteRenderer>(); 
    }
    private void Start()
    {
        SetColor(parent.Parent.team.teamColor);
    } 
    public void SetColor(Color color)
    {
        if (render) render.color = color;
    }
}
