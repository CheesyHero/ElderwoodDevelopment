using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class UI_RewardsPrinter : MonoBehaviour
{
    public TextMeshProUGUI text;
    public RewardsManager parent;
    public Button button;

    [Header("Main")]
    public float startDelay = 1.2f;
    public float intervalDelay = 0.8f;
    public float exitDelay = 1f;

    [Header("Typewriter")]
    public float characterDelay = 0.02f;

    private void Start()
    {
        if (!text || !parent || !button) return;

        StartCoroutine(PrintAnimation());
    }

    private IEnumerator PrintAnimation()
    {
        text.text = "";
        text.maxVisibleCharacters = 0;
        button.interactable = false;

        WaitForSecondsRealtime wait = new(intervalDelay);

        yield return new WaitForSecondsRealtime(startDelay);

        yield return TypeAppend($"Level Complete: <color=yellow>{parent.initialPayResult:0g}</color>\n");
        GameManager.Inventory.GainMoney(parent.initialPayResult);
        yield return wait;

        yield return SpeedRunPrint();
        yield return wait;

        yield return BasesCapturedPrint();
        yield return wait;

        if (RewardsManager.HasDominion)
        {
            yield return TypeAppend($"Dominion: <color=yellow>{parent.controlAllResult:0g}</color>\n");
            GameManager.Inventory.GainMoney(parent.controlAllResult);
            yield return wait;
        }

        yield return TypeAppend($"\n<u>Total = <color=yellow>{parent.grandTotalResult}g</color></u>");

        yield return new WaitForSecondsRealtime(exitDelay);

        button.interactable = true;
    }

    private IEnumerator TypeAppend(string addition)
    {
        int previousVisible = text.textInfo.characterCount;

        text.text += addition;
        text.ForceMeshUpdate();

        int targetVisible = text.textInfo.characterCount;
        text.maxVisibleCharacters = previousVisible;

        float timer = 0f;

        while (text.maxVisibleCharacters < targetVisible)
        {
            timer += Time.unscaledDeltaTime;

            int charsToReveal = Mathf.FloorToInt(timer / characterDelay);

            if (charsToReveal > 0)
            {
                text.maxVisibleCharacters = Mathf.Min(
                    text.maxVisibleCharacters + charsToReveal,
                    targetVisible
                );

                timer -= charsToReveal * characterDelay;
            }

            yield return null;
        }

        text.maxVisibleCharacters = targetVisible;
    }

    private IEnumerator SpeedRunPrint()
    {
        if (RewardsManager.HasSpeedRun)
        {
            yield return TypeAppend(
                $"Under {parent.speedrun} Turns " +
                $"(<color=green>{TurnManager.Round}</color>): " +
                $"<color=yellow>{parent.timeLimitResult:0g}</color>\n"
            );

            GameManager.Inventory.GainMoney(parent.timeLimitResult);
        }
        else
        {
            yield return TypeAppend(
                $"Under {parent.speedrun} Turns " +
                $"(<color=red>{TurnManager.Round}</color>): " +
                $"<color=grey>0g</color>\n"
            );
        }
    }

    private IEnumerator BasesCapturedPrint()
    {
        yield return TypeAppend(
            $"Bases Captured ({TeamBase.DominionCount(GameManager.Player.team) - 1} x {parent.payoutPerStronghold}g): " +
            $"<color=yellow>{parent.perStrongholdResult:0g}</color>\n"
        );

        GameManager.Inventory.GainMoney(parent.perStrongholdResult);
    }
}