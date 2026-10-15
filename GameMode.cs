using UnityEngine;

[System.Serializable]
public class GameMode
{
    // Default Game Mode is Conquest
    // If your home base becomes captured by an opponent, you are defeated
    // You must capture all opponents' home bases to win
    public string modeName = "Conquest";
    public override string ToString()
    { return modeName.Length > 0 ? modeName : base.ToString(); }

    public virtual void CheckGameOver()
    {
        Debug.Log("CHECKING WIN CONDITION");

        foreach(var army in EnemyArmyManager.AllArmies)
        {
            if (army.defeated == false && army.homeBase && army.homeBase.owner != army.team)
                EnemyArmyManager.Defeat(army);
        }

        var remaining = EnemyArmyManager.Remaining();
        if (remaining.Count <= 1)
        {
            HexCrawlManager.SetWinner(remaining[0]);
            TurnManager.EndGame();
        }
    }
    public virtual bool TieBreaker()
    {
        // If not multiplayer, the player will always lose to running out of turns.
        return false;
    }
    public virtual void PrintWinCondition()
    {
        BannerArray.PrintReport($"<color=blue><B>WIN CONDITION</b></color>: Capture the enemies' home base.");
    }
    public virtual void PrintFailCondition()
    {
        BannerArray.PrintReport($"<color=red><B>FAIL CONDITION</b></color>: An opponent captures your home base.");
    }
}
