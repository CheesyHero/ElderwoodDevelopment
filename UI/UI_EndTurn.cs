using UnityEngine;

public class UI_EndTurn : MonoBehaviour
{
    public void BUTTON_EndTurn()
    {
        TurnManager.EndTurn(GameManager.Player);
    }
}
