
using UnityEngine;
using UnityEngine.UI;

public class UI_EndTurn : MonoBehaviour
{
    public Image buttonImage;
    public Gradient gradient;

    [Header("Animation")]
    public float pulseSpeed = 1f;

    private Color defaultColor;

    private void Awake()
    {
        if (buttonImage != null)
            defaultColor = buttonImage.color;
    }

    private void Update()
    {
        if (buttonImage == null) return;

        bool readyToEnd =
            !HasAvailableMoves() &&
            !CanSpawnUnit() &&
            !CanPlayCard();

        if (readyToEnd)
        {
            float t = Mathf.PingPong(
                Time.unscaledTime * pulseSpeed, 1f
            );

            buttonImage.color = gradient.Evaluate(t);
        }
        else
        {
            buttonImage.color = defaultColor;
        }
    }

    private bool HasAvailableMoves()
    {
        return false;
    }

    private bool CanSpawnUnit()
    {
        return false;
    }

    private bool CanPlayCard()
    {
        return false;
    }

    public void BUTTON_EndTurn()
    {
        TurnManager.EndTurn(GameManager.Player);
    }
}
