using UnityEngine;

public enum E_CommandEffect
{
    DamageUnit = 0,
    HealUnit = 1,
    ReviveUnit = 2,
    ManaChange = 3,

    AddMovement = 5,
    FreezeMovement = 6,
    ApplyTeamEffect = 7,

    DrawCards = 10,
    MoveCardToNewZone = 11,
}

public enum E_CommandHealthValueType
{
    FlatValue = 0,
    PercentMaxHealth = 1,
    PercentCurrentHealth = 2,
    PercentMissingHealth = 3
}

[System.Serializable]
public class CommandEffects
{
    public E_CommandEffect effectType;
    public E_CommandHealthValueType valueType;

    public float value;
    public bool affectDefeated = false;

    public bool ApplyToWalker(HexWalker unit)
    {
        PrintPopUpAtWalker(unit);

        return effectType switch
        {
            E_CommandEffect.DamageUnit => ApplyToAllHeroesInUnit(unit.Parent),
            E_CommandEffect.HealUnit => ApplyToAllHeroesInUnit(unit.Parent),
            E_CommandEffect.ReviveUnit => ApplyToAllHeroesInUnit(unit.Parent),
            E_CommandEffect.ManaChange => ApplyToAllHeroesInUnit(unit.Parent),

            E_CommandEffect.AddMovement => AddMoveToWalker(unit),
            E_CommandEffect.FreezeMovement => FreezeWalkerMovement(unit),
            E_CommandEffect.ApplyTeamEffect => ApplyStatusToUnit(unit.Parent),

            _ => false
        };
    }

    public bool ApplyToUnit(Unit unit)
    {
        BannerArray.PrintReport($"Applying {effectType} {valueType} {value} to unit {unit}.");

        return effectType switch
        {
            E_CommandEffect.DamageUnit => ApplyToAllHeroesInUnit(unit),
            E_CommandEffect.HealUnit => ApplyToAllHeroesInUnit(unit),
            E_CommandEffect.ReviveUnit => ApplyToAllHeroesInUnit(unit),
            E_CommandEffect.ManaChange => ApplyToAllHeroesInUnit(unit),
            E_CommandEffect.ApplyTeamEffect => ApplyStatusToUnit(unit),

            _ => false
        };
    }

    public bool AddMoveToWalker(HexWalker unit)
    {
        unit.moves += Mathf.CeilToInt(value);
        return true;
    }

    public bool FreezeWalkerMovement(HexWalker unit)
    {
        unit.moves = 0;
        return true;
    }

    public bool ApplyStatusToUnit(Unit unit)
    {
        Passive effect = Passive.Get((int)value);

        BannerArray.PrintReport($"Applying <color=red>{effectType}</color> <color=green>{effect.skillName}</color> to unit {unit}.");

        unit.AddEffect(effect);
        return true;
    }

    public bool ApplyToAllHeroesInUnit(Unit unit)
    {
        bool applied = false;

        foreach (Hero hero in unit.heroes)
            if (ApplyToHero(hero))
                applied = true;

        return applied;
    }

    public bool ApplyToHero(Hero hero)
    {
        return effectType switch
        {
            E_CommandEffect.DamageUnit => DamageHero(hero),
            E_CommandEffect.HealUnit => HealHero(hero),
            E_CommandEffect.ReviveUnit => ReviveHero(hero),
            E_CommandEffect.ManaChange => ChangeHeroMana(hero),

            _ => false
        };
    }

    public bool DamageHero(Hero hero)
    {
        if (hero.health.IsDead)
            return false;

        int amount = GetHealthValue(hero);
        hero.health.health_current = Mathf.Max(0, hero.health.health_current - amount);

        return true;
    }

    public bool HealHero(Hero hero)
    {
        if (hero.health.IsDead && !affectDefeated)
            return false;

        int amount = GetHealthValue(hero);
        hero.health.health_current = Mathf.Min(hero.health.health_max, hero.health.health_current + amount);

        return true;
    }

    public bool ReviveHero(Hero hero)
    {
        if (hero.health.IsAlive)
            return false;

        int amount = Mathf.CeilToInt(Mathf.Clamp(GetHealthValue(hero), 1, hero.health.health_max));
        hero.health.health_current = amount;

        return true;
    }

    public bool ChangeHeroMana(Hero hero)
    {
        if (hero.health.IsDead && !affectDefeated)
            return false;

        hero.health.mana_current =
            Mathf.Clamp(hero.health.mana_current + GetManaValue(hero), 0, hero.health.mana_max);

        return true;
    }

    public int GetHealthValue(Hero hero)
    {
        float current = hero.health.health_current;
        float max = hero.health.health_max;
        float missing = max - current;
        float percent = value <= 1f ? value : value / 100f;

        float result = valueType switch
        {
            E_CommandHealthValueType.FlatValue => value,
            E_CommandHealthValueType.PercentMaxHealth => max * percent,
            E_CommandHealthValueType.PercentCurrentHealth => current * percent,
            E_CommandHealthValueType.PercentMissingHealth => missing * percent,
            _ => 0
        };

        return Mathf.CeilToInt(result);
    }

    public int GetManaValue(Hero hero)
    {
        float current = hero.health.mana_current;
        float max = hero.health.mana_max;
        float missing = max - current;
        float percent = value <= 1f ? value : value / 100f;

        float result = valueType switch
        {
            E_CommandHealthValueType.FlatValue => value,
            E_CommandHealthValueType.PercentMaxHealth => max * percent,
            E_CommandHealthValueType.PercentCurrentHealth => current * percent,
            E_CommandHealthValueType.PercentMissingHealth => missing * percent,
            _ => 0
        };

        return Mathf.CeilToInt(result);
    }

    public void PrintPopUpAtWalker(HexWalker unit)
    {
        Vector3 pos = unit.transform.position;

        switch (effectType)
        {
            case E_CommandEffect.DamageUnit:
                UI_PopUpTrigger.CreatePopUp($"-{value}", Color.red, pos, 64);
                break;

            case E_CommandEffect.HealUnit:
                UI_PopUpTrigger.CreatePopUp($"+{value}", Color.green, pos, 64);
                break;

            case E_CommandEffect.ReviveUnit:
                UI_PopUpTrigger.CreatePopUp("Revive", Color.cyan, pos, 64);
                break;

            case E_CommandEffect.ManaChange:
                UI_PopUpTrigger.CreatePopUp($"+{value} Mana", Color.blue, pos, 64);
                break;

            case E_CommandEffect.AddMovement:
                UI_PopUpTrigger.CreatePopUp($"+{value} Move", Color.yellow, pos, 64);
                break;

            case E_CommandEffect.FreezeMovement:
                UI_PopUpTrigger.CreatePopUp("Frozen", Color.blue, pos, 64);
                break;

            case E_CommandEffect.ApplyTeamEffect:
                Passive effect = Passive.Get((int)value);
                UI_PopUpTrigger.CreatePopUp($"{effect.skillName}", effect.color, pos, 64);
                break;
        }
    }
}