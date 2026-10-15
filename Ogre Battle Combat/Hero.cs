using System.Collections.Generic; 
using UnityEngine;

[System.Serializable]
public class Hero
{
    public string characterName = "New Hero";
    public Team team;
    public HeroClass heroClass;
    public int level = 1;
    public int experience = 0;
    public int nextLevelRequirement = 0;
    public Health health = new();
    public bool Defeated { get { return health.IsDead; } }
    public Attributes attributes = new();
    public BattlePlan plan = new();
    public List<Passive> passives = new();

    [Header("Equipment")]
    public Arsenal equipment = new();

    public List<Passive> ActivePassives { get { return new(passives); } }

    public override string ToString()
    {
        return characterName.Length > 0 ? characterName : base.ToString();
    }
    public Hero()
    {
        level = 1;
        heroClass = HeroClass.GetClass(0);
    }

    public Hero(int setLevel)
    {
        level = setLevel;
        heroClass = HeroClass.GetClass(0);
    }

    public Hero(int setLevel, HeroClass newClass)
    {
        level = setLevel;
        heroClass = newClass;
    }

    public Hero(int setLevel, int classIndex)
    {
        level = setLevel;
        heroClass = HeroClass.GetClass(classIndex);
    }

    public void Initialize()
    {
        if (level < 1) level = 1;
        if (level > 20) level = 20;
        if (heroClass == null) // failsafe
            heroClass = HeroClass.GetClass(0);

        AddStartingWeapons(characterName.Length > 0 && characterName[0] != '_');  
        SetAttributeLevels();
        SetHealth();
        plan.Initialize(this);

        passives.Clear();
        passives.AddRange(heroClass.passives);

        if (heroClass.leaderTraits.passive != null)
            passives.Add(heroClass.leaderTraits.passive);

        passives.AddRange(equipment.GetPassives());

        nextLevelRequirement = BattleConstants.GetEXPRequirement(level);
    }
    public void Refresh(float p, bool mana)
    {
        //SetAttributeLevels();
        health.Refresh(p, mana);

        //Debug.Log($"{this} has been refreshed {p * 100}%");
    }
    public void AddStartingWeapons(bool nodouplicate)
    {
        if (nodouplicate == true)
            return; // Character has not been created the first time

        else RollName();

        //Debug.Log($"{ToString()} : Adding starting items..."); 
        if (!equipment.weapon) equipment.weapon = heroClass.startingEquipment.weapon;
        if (!equipment.armor) equipment.armor = heroClass.startingEquipment.armor;
        if (!equipment.accessoryA) equipment.accessoryA = heroClass.startingEquipment.accessoryA;
        if (!equipment.accessoryB) equipment.accessoryB = heroClass.startingEquipment.accessoryB;
    }
    public void SetHealth()
    { 
        health.Initialize(this);
    }
    public Attributes CombinedAttributes
    { get { return attributes + equipment.GetCombinedAttributes(); } }

    public void SetAttributeLevels()
    {
        float oldPotential = attributes != null ? attributes.potential : -1;

        attributes = heroClass.GenerateStats(level);

        if (oldPotential > 0) 
            attributes.potential = oldPotential;
    }
    public void GainExperience(float gain)
    {
        if (Defeated)
        {
            gain *= 0.5f;
            health.mana_current = 0;
        } 
        experience += GetPotentialCurveCurve(gain);

        while (level < BattleConstants.maxLevel &&
               experience >= BattleConstants.GetEXPRequirement(level))
        {
            LevelUp();
        }
    }
    public int GetPotentialCurveCurve(float gain)
    {
        float x = attributes.potential / 100f; // should be between 1 and 100
        return Mathf.CeilToInt(gain * (1 + (x * x * x * x * BattleConstants.EXPCurve)) * Random.Range(0.9f, 1.1f));
    }
    public void LevelUp()
    {
        experience = 0;
        level = Mathf.Clamp(level + 1, 1, BattleConstants.maxLevel);
        Initialize();
    }
    public int GetReward()
    {
        return 1 + level * level;
    }
    public void RollName()
    {
        if (characterName == null || characterName.Length == 0 || characterName[0] == '_')
            characterName = heroClass.GenerateRandomName();
    }
    public bool CanUseEquipment(Equipment item)
    {
        if (item == null)
            return false;

        if (heroClass)
            return heroClass.CanUseEquipment(item);

        else return false;
    }
    public List<BattleMove> KnownSkills()
    {
        List<BattleMove> hits = new();
        hits.AddRange(heroClass.KnownSkills(level));
        hits.AddRange(equipment.GetAllSkills());

        return hits;
    }
    public void Promote()
    {
        if (!heroClass || !heroClass.promotion)
            return;

        // Debug.Log($"ATTEMPTING TO PROMOTE {this} of class {heroClass} to {heroClass.promotion}");

        heroClass.UsePromotionRequirement();
        heroClass = heroClass.promotion;

        // Debug.Log($"PROMOTION {this} is now class {heroClass}");

        Initialize();
        Refresh(1, true);
        plan.ValidateMoves(this);
    }

    public Hero Copy(HeroClass newClass = null)
    {
        Hero copy = new(level)
        {
            heroClass = newClass ? newClass : this.heroClass, // ScriptableObject: safe to share
            characterName = this.characterName,
            experience = this.experience,
            health = this.health.Copy(),           // must be deep
            attributes = this.attributes.Copy(),   // already implemented
            equipment = this.equipment.Copy(),     // ensure Arsenal has Copy()
            plan = this.plan.Copy(),                // ensure BattlePlan has Copy()
        };
        copy.Initialize(); 

        return copy;
    }

}
public enum Enum_HeroAttributes
{
    health = 0,
    mana = 1,
    power = 2,
    armor = 3,
    magic = 4,
    resist = 5,
    speed = 6,
    accuracy = 7,
    evasion = 8,
    shield = 9,
    actions = 10,
    reactions = 11,
    criticalChance = 12,
    criticalDamage = 13,
    potential = 14,
}

[System.Serializable]
public class Attributes
{
    public float health = 0;
    public float mana = 0;
    public float power = 0;
    public float armor = 0;
    public float magic = 0;
    public float resist = 0;
    public float speed = 0;
    public float accuracy = 0;
    public float evasion = 0;
    public float shield = 0;
    public int actions = 0;
    public int reactions = 0;
    public float criticalChance = 0;
    public float criticalDamage = 0;
    public float potential = 0; 

    public Attributes(float i = 0)
    {
        health = i;
        mana = i;
        power = i;
        armor = i;
        magic = i;
        resist = i;
        speed = i;
        accuracy = i;
        evasion = i;
        shield = i;
        actions = (int)i;
        reactions = (int)i;
        criticalChance = i;
        criticalDamage = i;
        potential = i;
    }

    public Attributes Copy()
    {
        Attributes copy = new()
        {
            health = this.health,
            mana = this.mana,
            power = this.power,
            armor = this.armor,
            magic = this.magic,
            resist = this.resist,
            speed = this.speed,
            accuracy = this.accuracy,
            evasion = this.evasion,
            shield = this.shield,
            actions = this.actions,
            reactions = this.reactions,
            criticalChance = this.criticalChance,
            criticalDamage = this.criticalDamage,
            potential = this.potential,
        };

        return copy;
    }

    public static string PrintAttributeName(Enum_HeroAttributes stat)
    {
        static string Colorize(string hex, string text)
            => $"<color=#{hex}>{text}</color>";

        return stat switch
        {
            Enum_HeroAttributes.health => Colorize("f03040", "Health"),
            Enum_HeroAttributes.mana => Colorize("c051f0", "Mana"),
            Enum_HeroAttributes.power => Colorize("ed463e", "Power"),
            Enum_HeroAttributes.armor => Colorize("3967db", "Armor"),
            Enum_HeroAttributes.magic => Colorize("d028ed", "Magic"),
            Enum_HeroAttributes.resist => Colorize("7239db", "Resist"),
            Enum_HeroAttributes.speed => Colorize("4cdb39", "Speed"),
            Enum_HeroAttributes.accuracy => Colorize("44e3bb", "Accuracy"),
            Enum_HeroAttributes.evasion => Colorize("36a9d6", "Evasion"),
            Enum_HeroAttributes.shield => Colorize("5189f0", "Shield"),
            Enum_HeroAttributes.actions => Colorize("f0603c", "Actions"),
            Enum_HeroAttributes.reactions => Colorize("38cdf2", "Reactions"),
            Enum_HeroAttributes.criticalChance => Colorize("fcd34c", "Crit Chance"),
            Enum_HeroAttributes.criticalDamage => Colorize("fcad4c", "Crit Damage"),
            Enum_HeroAttributes.potential => Colorize("a3e635", "Potential"),
            _ => stat.ToString(),
        };
    }

    public float MultiplyStat(Enum_HeroAttributes stat, float factor)
    {
        factor = Mathf.Max(0f, factor);

        switch (stat)
        {
            case Enum_HeroAttributes.health: health *= factor; return health;
            case Enum_HeroAttributes.mana: mana *= factor; return mana;
            case Enum_HeroAttributes.power: power *= factor; return power;
            case Enum_HeroAttributes.armor: armor *= factor; return armor;
            case Enum_HeroAttributes.magic: magic *= factor; return magic;
            case Enum_HeroAttributes.resist: resist *= factor; return resist;
            case Enum_HeroAttributes.speed: speed *= factor; return speed;
            case Enum_HeroAttributes.accuracy: accuracy *= factor; return accuracy;
            case Enum_HeroAttributes.evasion: evasion *= factor; return evasion;
            case Enum_HeroAttributes.shield: shield *= factor; return shield;
            case Enum_HeroAttributes.actions: actions = Mathf.RoundToInt(actions * factor); return actions;
            case Enum_HeroAttributes.reactions: reactions = Mathf.RoundToInt(reactions * factor); return reactions;
            case Enum_HeroAttributes.criticalChance: criticalChance *= factor; return criticalChance;
            case Enum_HeroAttributes.criticalDamage: criticalDamage *= factor; return criticalDamage;
            case Enum_HeroAttributes.potential: potential *= factor; return potential;
            default: return 0f;
        }
    }

    public float GetStat(Enum_HeroAttributes stat)
    {
        return stat switch
        {
            Enum_HeroAttributes.health => health,
            Enum_HeroAttributes.mana => mana,
            Enum_HeroAttributes.power => power,
            Enum_HeroAttributes.armor => armor,
            Enum_HeroAttributes.magic => magic,
            Enum_HeroAttributes.resist => resist,
            Enum_HeroAttributes.speed => speed,
            Enum_HeroAttributes.accuracy => accuracy,
            Enum_HeroAttributes.evasion => evasion,
            Enum_HeroAttributes.shield => shield,
            Enum_HeroAttributes.actions => actions,
            Enum_HeroAttributes.reactions => reactions,
            Enum_HeroAttributes.criticalChance => criticalChance,
            Enum_HeroAttributes.criticalDamage => criticalDamage,
            Enum_HeroAttributes.potential => potential,
            _ => 0f,
        };
    }

    public static Attributes operator +(Attributes a, Attributes b)
    {
        return new Attributes()
        {
            health = a.health + b.health,
            mana = a.mana + b.mana,
            power = a.power + b.power,
            armor = a.armor + b.armor,
            magic = a.magic + b.magic,
            resist = a.resist + b.resist,
            speed = a.speed + b.speed,
            accuracy = a.accuracy + b.accuracy,
            evasion = a.evasion + b.evasion,
            shield = a.shield + b.shield,
            actions = a.actions + b.actions,
            reactions = a.reactions + b.reactions,
            criticalChance = a.criticalChance + b.criticalChance,
            criticalDamage = a.criticalDamage + b.criticalDamage,
            potential = a.potential + b.potential,
        };
    }

    public static Attributes operator -(Attributes a, Attributes b)
    {
        return new Attributes()
        {
            health = a.health - b.health,
            mana = a.mana - b.mana,
            power = a.power - b.power,
            armor = a.armor - b.armor,
            magic = a.magic - b.magic,
            resist = a.resist - b.resist,
            speed = a.speed - b.speed,
            accuracy = a.accuracy - b.accuracy,
            evasion = a.evasion - b.evasion,
            shield = a.shield - b.shield,
            actions = a.actions - b.actions,
            reactions = a.reactions - b.reactions,
            criticalChance = a.criticalChance - b.criticalChance,
            criticalDamage = a.criticalDamage - b.criticalDamage,
            potential = a.potential - b.potential,
        };
    }

    public static Attributes operator *(Attributes a, Attributes b)
    {
        return new Attributes()
        {
            health = a.health * b.health,
            mana = a.mana * b.mana,
            power = a.power * b.power,
            armor = a.armor * b.armor,
            magic = a.magic * b.magic,
            resist = a.resist * b.resist,
            speed = a.speed * b.speed,
            accuracy = a.accuracy * b.accuracy,
            evasion = a.evasion * b.evasion,
            shield = a.shield * b.shield,
            actions = a.actions * b.actions,
            reactions = a.reactions * b.reactions,
            criticalChance = a.criticalChance * b.criticalChance,
            criticalDamage = a.criticalDamage * b.criticalDamage,
            potential = a.potential * b.potential,
        };
    }
}

