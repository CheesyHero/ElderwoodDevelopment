using UnityEngine;

public class HeroHealthDisplay : MonoBehaviour
{
    public BattleCombatant parent;
    public UI_HealthBar healthBar;

    private void Update()
    {
        if (!parent) return;

        if (healthBar)
            healthBar.UpdateFill(parent);
    }
}
