using UnityEngine;

public class UnitHeroHighlight : MonoBehaviour
{
    public BattleTile parent;
    public SpriteRenderer sprite; 

    private void Awake()
    {
        if(!parent)
            parent = GetComponentInParent<BattleTile>();

        if(!sprite)
            sprite = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (parent == null || parent.CurrentHero == null)
            sprite.color = Color.white / 2;

        else if(parent.CurrentHero.IsAlive)
        {
            if (sprite.color != parent.CurrentHero.team.teamColor)
                sprite.color = parent.CurrentHero.team.teamColor;
        }

        else sprite.color = Color.white / 2;
    }
}
