using UnityEngine;

[System.Serializable]
public class Health
{
    public float health_current;
    public float health_max = 100;
    public float shield_current;
    public float shield_max = 100;
    public float mana_current;
    public float mana_max = 100;

    public bool IsAlive { get { return health_current > 0; } }
    public bool IsDead { get { return health_current <= 0; } }
    public float HealthPercent { get { return health_max > 0 ? health_current / health_max : 0; } }
    public float ShieldPercent { get { return shield_max > 0 ? shield_current / shield_max : 0; } }
    public float ManaPercent { get { return mana_max > 0 ? mana_current / mana_max : 0; } }


    public void Refresh(float percent = 1f, bool manaClear = false)
    {
        health_current = Mathf.Clamp(health_current + health_max * percent, 0, health_max);
        shield_current = 0;
        if(manaClear) mana_current = 0;
    }

    public void Clamp()
    { 
        health_current = Mathf.Clamp(health_current, 0, health_max); 
        shield_current = Mathf.Clamp(shield_current, 0, shield_max);
        mana_current = Mathf.Clamp(mana_current, 0, mana_max);
    }


    public void DamageHealth(BattleEvent input)
    {
        if (input == null || input.value <= 0f) return;

        DamageShield(ref input);
        LoseHealth(input);

        Clamp();
    }
    public void GainHealth(BattleEvent input)
    {
        if (input == null || input.value <= 0f) return;

        health_current += input.value;
        Clamp();
    }

    public void LoseHealth(BattleEvent input)
    {
        if (input == null || input.value <= 0f) return;

        health_current -= input.value;
        GenerateMana(input.value, 0.5f);

        Clamp();
    }


    public void GainShield(BattleEvent input)
    {
        if (input == null || input.value <= 0f) return;

        shield_current += input.value;
        Clamp();
    }
    public void DamageShield(ref BattleEvent e)
    {
        if (e == null || e.value <= 0f) return;
        if (shield_current <= 0f) return;

        float absorbed = Mathf.Min(shield_current, e.value);
        shield_current -= absorbed;

        // Remaining damage passes through to health
        e.value -= absorbed;

        // Generate mana based on shield absorbed
        if (shield_max > 0f)
            mana_current += absorbed / shield_max * 75f;

        Clamp();
    }

    private float GetShieldMitigationStat(BattleEvent e)
    {
        if (e == null || e.owner == null || e.owner.hero == null || e.move == null) return 1f;

        if (e.move.damageType == DamageTypes.magical)
            return e.owner.Attributes.resist;

        else return e.owner.Attributes.armor;
    }


    public void DrainMana(BattleEvent input)
    {
        if (input == null || input.value <= 0) return;

        mana_current -= input.value;

        Clamp();
    }

    public void GainMana(BattleEvent input)
    {
        if (input == null || input.value <= 0) return;

        mana_current += input.value;

        Clamp();
    }

    public void GainMana(float value)
    {
        if (value <= 0) return;

        mana_current += value;

        Clamp();
    }
    public void GenerateMana(float weight, float mult = 0.5f)
    {
        float generation = weight / health_max * mult;
        mana_current += generation * 100f;

        Clamp();
    }
    public void Initialize(Hero hero)
    {
        if (hero == null) return;

        if (!hero.heroClass)
            health_max = mana_max = 100f + hero.level * 10f;

        else
        {
            health_max =
                hero.heroClass.baseStats.health +
                hero.heroClass.statGrowth.health * hero.level +
                hero.equipment.GetTotalValue(Enum_HeroAttributes.health);
            shield_max =
                hero.heroClass.baseStats.health +
                hero.heroClass.statGrowth.health * hero.level +
                hero.equipment.GetTotalValue(Enum_HeroAttributes.health);

            mana_max =
                hero.heroClass.baseStats.mana +
                hero.heroClass.statGrowth.mana * hero.level +
                hero.equipment.GetTotalValue(Enum_HeroAttributes.mana);
        }
    }
    public Health Copy()
    {
        return new Health
        {
            health_current = this.health_current,
            health_max = this.health_max,
            shield_current = this.shield_current,
            shield_max = this.shield_max,
            mana_current = this.mana_current,
            mana_max = this.mana_max,
        };
    }

}
