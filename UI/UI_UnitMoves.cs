using UnityEngine;
using TMPro;
using Unity.VisualScripting;

public class UI_UnitMoves : MonoBehaviour
{
    public HexWalker parent;
    public TextMeshPro moveText;
    public TextMeshPro fatigueText;

    // Update is called once per frame
    private void Start()
    {
        if (moveText && parent)
        {
            moveText.colorGradient = new()
            { 
                topLeft = Color.white, 
                topRight = Color.white, 
                bottomLeft = parent.Team.teamColor,
                bottomRight = parent.Team.teamColor,
            };
        }
    }
    void Update()
    {
        if (!parent) return;

        if(moveText) 
            moveText.text = parent.Army == EnemyArmyManager.CurrentTurnArmy() ? parent.moves.ToString("0") : ""; 

        if(fatigueText)
        {
            fatigueText.text = parent.fatigue > 0 ? parent.fatigue.ToString("0") : "";
        }
    }
}
