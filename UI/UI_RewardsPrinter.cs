
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

        // GOLD REWARDS
        yield return TypeAppend(
            "<color=yellow><b>GOLD REWARDS</b></color>\n"
        );

        yield return TypeAppend(
            $"Level Complete: <color=yellow>{parent.initialPayResult}g</color>\n"
        );
        GameManager.Inventory.GainMoney(parent.initialPayResult);
        yield return wait;

        yield return SpeedRunPrint();
        yield return wait;

        yield return BasesCapturedPrint();
        yield return wait;

        if (RewardsManager.HasDominion)
        {
            yield return TypeAppend(
                $"Dominion: <color=yellow>{parent.controlAllResult}g</color>\n"
            );

            GameManager.Inventory.GainMoney(parent.controlAllResult);
            yield return wait;
        }

        yield return TypeAppend(
            $"\n<u>Total Gold: <color=yellow>{parent.grandTotalResult}g</color></u>\n"
        );

        yield return wait;

        // EXPERIENCE REWARDS
        yield return ExperiencePrint();
        yield return wait;

        // ITEM REWARDS
        yield return ItemRewardsPrint();

        yield return new WaitForSecondsRealtime(exitDelay);

        button.interactable = true;
    }

    private IEnumerator SpeedRunPrint()
    {
        if (RewardsManager.HasSpeedRun)
        {
            yield return TypeAppend(
                $"Under {parent.speedrun} Turns " +
                $"(<color=green>{TurnManager.Round}</color>): " +
                $"<color=yellow>{parent.timeLimitResult}g</color>\n"
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
            $"Bases Captured " +
            $"({TeamBase.DominionCount(GameManager.Player.team) - 1} " +
            $"x {parent.payoutPerStronghold}g): " +
            $"<color=yellow>{parent.perStrongholdResult}g</color>\n"
        );

        GameManager.Inventory.GainMoney(parent.perStrongholdResult);
    }

    private IEnumerator ExperiencePrint()
    {
        Army player = GameManager.Player;

        int armyEXP = 50 + GameManager.Distance * 5;
        int defenderEXP =
            EnemyArmyManager.GetOtherKnockOuts(player) * 10;

        yield return TypeAppend(
            "\n<color=green><b>EXPERIENCE REWARDS</b></color>\n"
        );

        yield return TypeAppend(
            $"Army Experience: " +
            $"<color=green>+{armyEXP} EXP</color> per hero\n"
        );

        if (player.homeBase.CurrentUnit != null)
        {
            yield return TypeAppend(
                $"Home Defense: " +
                $"<color=green>+{defenderEXP} EXP</color> per defender\n"
            );
        }
    }

    private IEnumerator ItemRewardsPrint()
    {
        yield return TypeAppend(
            "\n<color=yellow><b>ITEM REWARDS</b></color>\n"
        );

        yield return TypeAppend(
            $"Reward Selections: " +
            $"<color=yellow>{parent.totalRewards}</color>\n"
        );
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

            int charsToReveal =
                Mathf.FloorToInt(timer / characterDelay);

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
}
