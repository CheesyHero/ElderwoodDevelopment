
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

    [Header("Audio")]
    public AudioClip experienceAudio, defenderAudio;
    public AudioClip rewardsAudio1, rewardsAudio2, rewardsAudio3;

    private WaitForSecondsRealtime wait;

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

        wait = new(intervalDelay);

        yield return new WaitForSecondsRealtime(startDelay);

        // GOLD REWARDS
        yield return TypeAppend("<color=yellow><b>GOLD REWARDS</b></color>\n");

        yield return PrintReward("Level Complete", parent.initialPayResult);

        yield return SpeedRunPrint();
        yield return wait;

        yield return BasesCapturedPrint();
        yield return wait;

        yield return PrintReward("Dominion", parent.controlAllResult);

        yield return PrintReward("Bounties", parent.totalBonusResult, RewardsManager.Bonuses.ToString());

        yield return TypeAppend($"\n<u>Total Gold: <color=yellow>{parent.grandTotalResult}g</color></u>\n");
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
        int defenderEXP = EnemyArmyManager.GetOtherKnockOuts(player) * 10;

        PlayAudio(experienceAudio);

        yield return TypeAppend("\n<color=green><b>EXPERIENCE REWARDS</b></color>\n");

        yield return TypeAppend(
            $"Army Experience: <color=green>+{armyEXP} EXP</color> per hero\n"
        );

        if (player.homeBase.CurrentUnit != null)
        {
            PlayAudio(defenderAudio);

            yield return TypeAppend(
                $"Home Defense: <color=green>+{defenderEXP} EXP</color> per defender\n"
            );
        }
    }

    private IEnumerator ItemRewardsPrint()
    {
        yield return TypeAppend("\n<color=yellow><b>ITEM REWARDS</b></color>\n");

        AudioClip rewardClip = parent.totalRewards switch
        {
            1 => rewardsAudio1,
            2 => rewardsAudio2,
            3 => rewardsAudio3,
            _ => null
        };

        PlayAudio(rewardClip);

        yield return TypeAppend(
            $"Reward Selections: <color=yellow>{parent.totalRewards}</color>\n"
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

            int charsToReveal = Mathf.FloorToInt(timer / characterDelay);

            if (charsToReveal > 0)
            {
                text.maxVisibleCharacters = Mathf.Min(text.maxVisibleCharacters + charsToReveal, targetVisible);
                timer -= charsToReveal * characterDelay;
            }

            yield return null;
        }

        text.maxVisibleCharacters = targetVisible;
    }

    private IEnumerator PrintReward(string title, int amount, string parentheses = "", Color? color = null)
    {
        if (amount <= 0) yield break;

        Color selectedColor = color ?? Color.green;
        string hexColor = ColorUtility.ToHtmlStringRGB(selectedColor);

        string extra = string.IsNullOrWhiteSpace(parentheses)
            ? ""
            : $" (<color=#{hexColor}>{parentheses}</color>)";

        yield return TypeAppend($"{title}{extra}: <color=yellow>{amount}g</color>\n");

        GameManager.Inventory.GainMoney(amount);

        yield return wait;
    }

    private void PlayAudio(AudioClip clip, float volume = 1f, bool modulate = true)
    {
        if (!clip) return;

        AudioInstancer.PlayAudioInstance(clip, volume, modulate);
    }
}
