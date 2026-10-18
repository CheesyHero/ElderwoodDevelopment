using UnityEngine;

public class UI_BattleHeroLevelRank : ComponentLoader
{
    public BattleCombatant parent;

    private void Awake()
    {
        if(!parent) parent = GetComponentInParent<BattleCombatant>();
    }

    private void Start()
    {
        if (!parent) return;

        SetText(parent.hero.level.ToString());
        SetImageColor(parent.hero.heroClass.RankColor());
    }
}
