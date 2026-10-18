using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class UI_HealthBar : MonoBehaviour
{ 
    public Image health;
    public Image mana;
    public Image armor;
    public void UpdateFill(BattleCombatant parent)
    {
        if (parent == null) return;

        if (health) health.fillAmount = parent.hero.health.HealthPercent;
        if (mana) mana.fillAmount = parent.hero.health.ManaPercent;
        if (armor) armor.fillAmount = parent.hero.health.ShieldPercent;
    }
}
