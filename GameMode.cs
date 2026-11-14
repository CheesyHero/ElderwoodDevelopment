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

        // alternative player lose condition //
        if (HexCrawlManager.TurnTimer > 0 && HexCrawlManager.TurnsRemaining <= 0)
        {
            // player loses
            Debug.LogWarning($"TIME UP! timer = {HexCrawlManager.TurnTimer} - turns remaining = {HexCrawlManager.TurnsRemaining} ({HexCrawlManager.TurnTimer} - {TurnManager.Round})");
            EnemyArmyManager.Defeat(GameManager.Player);
        }

        foreach(var army in EnemyArmyManager.AllArmies)
        {
            // Check all participants to see if they still own their home base
            if (army.defeated == false && army.homeBase && army.homeBase.owner != army.team)
                EnemyArmyManager.Defeat(army); // If not, they are eleminated and all their units are removed
        }

        var remaining = EnemyArmyManager.Remaining();
        if (remaining.Count <= 1) // End the game if only 1 army remains
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
