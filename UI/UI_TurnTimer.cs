
using UnityEngine;
using TMPro;

public class UI_TurnTimer : MonoBehaviour
{
    public TextMeshProUGUI textMesh;

    [Header("Countdown Gradient")]
    public Gradient countdownGradient;
    public int startingTurns = 10;

    [Header("Final Turn Animation")]
    public Gradient finalTurnGradient;
    public float pulseSpeed = 1f;

    private bool isFinalTurn = false;

    private void Awake()
    {
        TurnManager.OnEventTrigger += UpdateText;
    }

    private void Start()
    {
        UpdateText();
    }

    private void OnDestroy()
    {
        TurnManager.OnEventTrigger -= UpdateText;
    }

    private void Update()
    {
        if (!textMesh || !isFinalTurn) return;

        float t = Mathf.PingPong(
            Time.unscaledTime * pulseSpeed, 1f
        );

        textMesh.color = finalTurnGradient.Evaluate(t);
    }

    private void UpdateText(string _) => UpdateText();

    public void UpdateText()
    {
        if (!textMesh) return;

        int remaining = HexCrawlManager.TurnsRemaining;

        isFinalTurn = remaining == 1;

        if (isFinalTurn)
        {
            textMesh.text = "Final Turn";
            textMesh.color = finalTurnGradient.Evaluate(0f);
        }
        else
        {
            textMesh.text = $"{remaining} Turns Remaining";

            // 10 turns = 0, 1 turn = 1
            float t = Mathf.InverseLerp(
                startingTurns,
                1f,
                remaining
            );

            textMesh.color = countdownGradient.Evaluate(t);
        }
    }
}
