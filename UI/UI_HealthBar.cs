
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_HealthBar : MonoBehaviour
{
    [Header("Health")]
    public Image health_fore;
    public Image health_backing;
    public TextMeshProUGUI textMesh;

    [Header("Shield")]
    public Image armor;
    public GameObject shieldHandle;
    public TextMeshProUGUI shieldText;

    [Header("Mana")]
    public Image mana;

    [Header("Animation")]
    [SerializeField] private float animationSpeed = 8f;
    [SerializeField] private float minAnimationSpeed = 0.25f;
    [SerializeField] private float textSpeed = 12f;

    private bool initialized = false;
    private bool goingDown = true;

    private float previousHealth;
    private float targetHealth;

    private float displayedHealth;
    private float displayedShield;

    private BattleCombatant currentParent;

    public void UpdateFill(BattleCombatant parent)
    {
        if (parent == null || parent.hero == null) return;

        var stats = parent.hero.health;

        float currentHealth = stats.HealthPercent;
        float currentMana = stats.ManaPercent;
        float currentArmor = stats.ShieldPercent;

        float healthValue = stats.health_current;
        float maxHealth = stats.health_max;
        float shieldValue = stats.shield_current;
        float maxShield = stats.shield_max;

        // Initialize immediately when owner changes
        if (!initialized || currentParent != parent)
        {
            initialized = true;
            currentParent = parent;

            previousHealth = currentHealth;
            targetHealth = currentHealth;
            goingDown = true;

            displayedHealth = healthValue;
            displayedShield = shieldValue;

            if (health_fore)
                health_fore.fillAmount = currentHealth;

            if (health_backing)
            {
                health_backing.fillAmount = currentHealth;
                health_backing.color = Color.white;
            }

            if (mana)
                mana.fillAmount = currentMana;

            if (armor)
                armor.fillAmount = currentArmor;
        }

        // Detect damage or healing
        if (!Mathf.Approximately(currentHealth, previousHealth))
            goingDown = currentHealth < previousHealth;

        targetHealth = currentHealth;
        previousHealth = currentHealth;

        // Health bar
        UpdateHealthBar();

        // Mana bar
        if (mana)
        {
            mana.fillAmount = SmoothFill(
                mana.fillAmount,
                currentMana
            );
        }

        // Shield bar
        if (armor)
        {
            armor.fillAmount = SmoothFill(
                armor.fillAmount,
                currentArmor
            );
        }

        // Shield visibility
        if (shieldHandle)
        {
            bool hasShield = shieldValue >= 1f;

            if (shieldHandle.activeSelf != hasShield)
                shieldHandle.SetActive(hasShield);
        }

        // Health text
        AnimateText(
            textMesh,
            ref displayedHealth,
            healthValue,
            maxHealth, false
        );

        // Shield text
        AnimateText(
            shieldText,
            ref displayedShield,
            shieldValue,
            maxShield, false
        );
    }

    private void UpdateHealthBar()
    {
        if (!health_fore || !health_backing) return;

        if (goingDown)
        {
            // Damage: foreground updates immediately
            health_fore.fillAmount = targetHealth;

            // White backing catches up
            health_backing.color = Color.white;

            health_backing.fillAmount = SmoothFill(
                health_backing.fillAmount,
                targetHealth
            );
        }
        else
        {
            // Healing: backing updates immediately
            health_backing.fillAmount = targetHealth;
            health_backing.color = Color.green;

            // Foreground catches up
            health_fore.fillAmount = SmoothFill(
                health_fore.fillAmount,
                targetHealth
            );
        }

        // Reset backing to white when idle
        if (Mathf.Approximately(
            health_fore.fillAmount,
            health_backing.fillAmount))
        {
            health_fore.fillAmount = targetHealth;
            health_backing.fillAmount = targetHealth;
            health_backing.color = Color.white;
        }
    }

    private float SmoothFill(float current, float target)
    {
        float t = 1f - Mathf.Exp(
            -animationSpeed * Time.deltaTime
        );

        float lerped = Mathf.Lerp(current, target, t);

        float minimumStep = minAnimationSpeed * Time.deltaTime;

        return Mathf.MoveTowards(
            current,
            target,
            Mathf.Max(
                Mathf.Abs(lerped - current),
                minimumStep
            )
        );
    }


    private void AnimateText(TextMeshProUGUI mesh, ref float current, float target, float maximum, bool showMax)
    {
        if (!mesh) return;

        float t = 1f - Mathf.Exp(
            -textSpeed * Time.deltaTime
        );

        float lerped = Mathf.Lerp(current, target, t);

        float minimumStep = minAnimationSpeed *
            Mathf.Max(1f, maximum) * Time.deltaTime;

        current = Mathf.MoveTowards(
            current,
            target,
            Mathf.Max(
                Mathf.Abs(lerped - current),
                minimumStep
            )
        );

        string value = $"<b>{Mathf.CeilToInt(current)}</b>";

        if (showMax)
            value += $"<size=0.2>/{Mathf.CeilToInt(maximum)}";

        mesh.text = value;
    }

}
