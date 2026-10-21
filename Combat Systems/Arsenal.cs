using UnityEngine;
using System.Collections.Generic;
using System;

[System.Serializable]
public class Arsenal
{
    public Weapon weapon;
    public Armor armor;
    public Equipment accessoryA;
    public Equipment accessoryB;

    public void ChangeWeapon(Weapon next, Inventory returnTo)
    { 
        returnTo?.AddItem(weapon);

        weapon = next;

        returnTo?.RemoveItem(next);

        Debug.Log($"Updated weapon to {weapon}");
    }

    public void ChangeArmor(Armor next, Inventory returnTo)
    { 
        returnTo?.AddItem(armor);

        armor = next;

        returnTo?.RemoveItem(next);

        Debug.Log($"Updated armor to {armor}");
    }

    public void ChangeAccessoryA(Equipment next, Inventory returnTo)
    { 
        returnTo?.AddItem(accessoryA);

        accessoryA = next;

        returnTo?.RemoveItem(next);

        Debug.Log($"Updated accessory A to {accessoryA}");
    }

    public void ChangeAccessoryB(Equipment next, Inventory returnTo)
    { 
        returnTo?.AddItem(accessoryB);

        accessoryB = next;

        returnTo?.RemoveItem(next);

        Debug.Log($"Updated accessory B to {accessoryB}");
    } 
    public Attributes GetCombinedAttributes()
    {
        return (weapon ? weapon.stats : new()) +
            (armor ? armor.stats : new()) +
            (accessoryA ? accessoryA.stats : new()) +
            (accessoryB ? accessoryB.stats : new());
    }
    public float GetTotalValue(Enum_HeroAttributes attribute)
    {
        Attributes total =
            (weapon ? weapon.stats : new()) +
            (armor ? armor.stats : new()) +
            (accessoryA ? accessoryA.stats : new()) +
            (accessoryB ? accessoryB.stats : new());

        return attribute switch
        {
            Enum_HeroAttributes.health => total.health,
            Enum_HeroAttributes.mana => total.mana,
            Enum_HeroAttributes.power => total.power,
            Enum_HeroAttributes.armor => total.armor,
            Enum_HeroAttributes.magic => total.magic,
            Enum_HeroAttributes.resist => total.resist,
            Enum_HeroAttributes.speed => total.speed,
            Enum_HeroAttributes.accuracy => total.accuracy,
            Enum_HeroAttributes.evasion => total.evasion,
            Enum_HeroAttributes.shield => total.shield,
            Enum_HeroAttributes.actions => total.actions,
            Enum_HeroAttributes.reactions => total.reactions,
            _ => 0f,
        };
    }

    internal BattleMove[] GetAllSkills()
    {
        List<BattleMove> finds = new();

        if(weapon && weapon.skills.Length > 0) finds.AddRange(weapon.skills); 
        if(armor && armor.skills.Length > 0) finds.AddRange(armor.skills);
        if(accessoryA && accessoryA.skills.Length > 0) finds.AddRange(accessoryA.skills);
        if(accessoryB && accessoryB.skills.Length > 0) finds.AddRange(accessoryB.skills);

        return finds.ToArray();
    }

    public Modifier[] GetAllMods()
    {
        List<Modifier> finds = new();

        if(weapon && weapon.effects.Length > 0) 
            finds.AddRange(ExtractMods(weapon)); 

        if(armor && armor.effects.Length > 0)
            finds.AddRange(ExtractMods(armor));

        if (accessoryA && accessoryA.effects.Length > 0)
            finds.AddRange(ExtractMods(accessoryA));

        if (accessoryB && accessoryB.effects.Length > 0)
            finds.AddRange(ExtractMods(accessoryB));

        return finds.ToArray();
    }
    public List<Modifier> ExtractMods(Equipment equip)
    {
        List<Modifier> finds = new();

        if (equip == null || equip.effects.Length == 0) 
            return new();

        foreach (var effect in equip.effects)
            foreach (var mod in effect.modifiers)
                finds.Add(mod);

        return finds;
    }
    public Arsenal Copy()
    {
        return new Arsenal
        {
            weapon = this.weapon,
            armor = this.armor,
            accessoryA = this.accessoryA,
            accessoryB = this.accessoryB,
        };
    }

    public Passive[] GetPassives()
    {
        List<Passive> passives = new List<Passive>();

        if (weapon) passives.AddRange(weapon.effects);
        if (armor) passives.AddRange(armor.effects);
        if (accessoryA) passives.AddRange(accessoryA.effects);
        if (accessoryB) passives.AddRange(accessoryB.effects);

        return passives.ToArray();
    }
}
