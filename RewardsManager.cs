using UnityEngine;
using TMPro;

public class RewardsManager : MonoBehaviour
{
    public int initialPayResult = 0;
    public int timeLimitResult = 0;
    public int controlAllResult = 0;
    public int perStrongholdResult = 0;
    public int grandTotalResult = 0;

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

    [Header("Tools")]
    public TextMeshProUGUI resultsDisplay;
    public Item basicReward;
    public Item bossReward;

    public static bool HasSpeedRun;
    public static bool HasDominion;
    
    private void OnEnable()
    {
        CountRewards();
    }
    private void Update()
    {
        if (resultsDisplay)
            resultsDisplay.text = $"Pick {totalRewards - used} rewards!";
    }

    public void CountRewards()
    {
        used = 0;

        HasSpeedRun = TurnManager.Round <= speedrun;
        HasDominion = TeamBase.HasDominion(GameManager.Player.team);

        initialPayResult = basePayout * (GameManager.CurrentLevel + 1);
        timeLimitResult = HasSpeedRun ? payoutForSpeedrun : 0;
        controlAllResult = HasDominion ? payoutForDominion : 0;
        perStrongholdResult = (TeamBase.DominionCount(GameManager.Player.team) - 1) * payoutPerStronghold;

        grandTotalResult = initialPayResult + timeLimitResult + controlAllResult + perStrongholdResult;

        totalRewards = Mathf.Min(1 + (HasSpeedRun ? 1 : 0) + (HasDominion ? 1 : 0), max);
    }
    public void MoveToNext()
    {
        used++;

        if (used >= totalRewards)
            Skip();
    }
    public void Skip()
    {
        if (GameManager.CurrentEncounter.depth < 5)
            GameManager.Inventory.AddItem(basicReward);

        else
        {
            GameManager.Inventory.AddItem(bossReward ? basicReward : bossReward);
            GameManager.NextLevel();
        }

        ExperienceBonus();
        //GameManager.Inventory.GainMoney(grandTotalPayout); 
        SceneChanger.ChangeScenes(nextScene);
    }

    public void ExperienceBonus()
    {
        Army player = GameManager.Player;

        if(player.homeBase.CurrentUnit != null)
            foreach (var item in player.homeBase.CurrentUnit.Parent.heroes)
                item.GainExperience(EnemyArmyManager.GetOtherKnockOuts(player) * 10); // home defender bonus

        foreach (var item in player.party)
            item.GainExperience(50 + GameManager.Distance * 5); // all army bonus
    }
}
