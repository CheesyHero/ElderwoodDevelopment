using UnityEngine;

public class TreasureEnemy : HexWalker
{
    public ActiveMenu defeatMenu;

    private bool defeatedByPlayer = false;
    private bool eliminated = false;

    public override void Defeat(Army opponent, bool always = false)
    {
        if (Parent.IsDefeated() || always)
            defeatedByPlayer |= opponent == GameManager.Player;

        base.Defeat(opponent, always);
    }

    public override void Eliminate()
    {
        if (eliminated) return;
        eliminated = true;

        if (defeatedByPlayer && defeatMenu != null)
            Instantiate(defeatMenu);

        base.Eliminate();
    }
}