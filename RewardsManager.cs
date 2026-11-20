using UnityEngine;
using TMPro;

public class RewardsManager : MonoBehaviour
{
    public bool oneShot = false;

    [Header("Main")]
    public int initialPayResult = 0;
    public int timeLimitResult = 0;
    public int controlAllResult = 0;
    public int perStrongholdResult = 0;
    public int grandTotalResult = 0;
    public int totalBonusResult = 0;

    [Header("Input Values")]
    public int nextScene = 0;
    public int max = 2;
    public int totalRewards = 1;
    public int used = 0;

    [Header("Speed Run Reward Score")]
    public float speedrun = 6;

    [Header("Gold Rewards")]
    public int basePayout = 50;
    public int payoutPerStronghold = 25;
    public int payoutForSpeedrun = 100;
    public int payoutForDominion = 100;
    public int payoutBonusPerReward = 100;

    [Header("Tools")]
    public TextMeshProUGUI resultsDisplay;
    public Item basicReward;
    public Item bossReward;

    public static bool HasSpeedRun;
    public static bool HasDominion;

    private static int bonusRewards = 0;
    public static int Bonuses { get { return bonusRewards; } }

    private void OnEnable()
    {
        CountRewards();
    }

    private void Update()
    {
        if (resultsDisplay)
            resultsDisplay.text = $"Pick {Mathf.Max(0, totalRewards - used)} rewards!";
    }

    public void CountRewards()
    {
        used = 0;

        // One Shot: exactly one reward, no payout.
        if (oneShot)
        {
            totalRewards = 1;

            initialPayResult = 0;
            timeLimitResult = 0;
            controlAllResult = 0;
            perStrongholdResult = 0;
            grandTotalResult = 0;
            totalBonusResult = 0;

            HasSpeedRun = false;
            HasDominion = false;

            bonusRewards++;

            return;
        }

        HasSpeedRun = TurnManager.Round <= speedrun;
        HasDominion = TeamBase.HasDominion(GameManager.Player.team);

        initialPayResult = basePayout * (GameManager.CurrentLevel + 1);
        timeLimitResult = HasSpeedRun ? payoutForSpeedrun : 0;
        controlAllResult = HasDominion ? payoutForDominion : 0;
        perStrongholdResult =
            (TeamBase.DominionCount(GameManager.Player.team) - 1)
            * payoutPerStronghold;


        totalBonusResult = bonusRewards * payoutBonusPerReward;

        grandTotalResult =
            initialPayResult +
            timeLimitResult +
            controlAllResult +
            perStrongholdResult +
            totalBonusResult;

        totalRewards = Mathf.Min(
            1 + (HasSpeedRun ? 1 : 0) + (HasDominion ? 1 : 0),
            max
        );

        bonusRewards = 0;
    }

    public void MoveToNext()
    {
        used++;

        if (used >= totalRewards)
            Skip();
    }

    public void Skip()
    {
        if (oneShot)
        {
            Destroy(gameObject);
            return;
        }

        if (GameManager.CurrentEncounter.depth < 5)
        {
            // GameManager.Inventory.AddItem(basicReward);
        }
        else
        { 
            GameManager.NextLevel();
            bonusRewards = 0;
        }

        ExperienceBonus();

        // GameManager.Inventory.GainMoney(grandTotalResult);

        FlagManager.AddFlag("First Game", FlagType.permanant);
        SceneChanger.ChangeScenes(nextScene);
    }

    public void ExperienceBonus()
    {
        Army player = GameManager.Player;

        if (player.homeBase.CurrentUnit != null)
        {
            foreach (var item in player.homeBase.CurrentUnit.Parent.heroes)
                item.GainExperience(
                    EnemyArmyManager.GetOtherKnockOuts(player) * 10
                );
        }

        foreach (var item in player.party)
            item.GainExperience(50 + GameManager.Distance * 5);
    } 
}