using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Enum_HeroClass_Tags
{
    rogue, warrior, mage, bard, cleric, support, tank, flying, ranged, melee, aquatic, undead, assassin,
}

[CreateAssetMenu(fileName = "New Class", menuName = "NMD/RPG/Class", order = 5)]
public class HeroClass : ScriptableObject
{
    [Tooltip("A Legendary Hero will always be the leader of their Unit.")]
    public RarityOptions rarity;
    [Range(1, 4)]
    public int rank = 1;
    [Header("Display Info")]
    public string className = "new class";
    [TextArea(1, 8)]
    public string description;
    public Sprite actorImage;
    public Texture2D colorMask;
    [Range(1, 5)]
    [Tooltip("How many placements in a Unit does this hero occupy?")]
    public int size = 1;
    [Tooltip("A hero that is Unit Locked cannot be removed from a unit by normal means through the Tavern menu.")]
    public bool unitLocked = false;
    [Header("Stats")]
    public Attributes baseStats;
    public Attributes statGrowth;
    [Header("Weapons")]
    public List<Enum_Equipment_Types> usableEquipment;
    public Arsenal startingEquipment;
    [Header("Skills")]
    public List<LearnableSkill> skills = new();
    public List<Passive> passives = new();
    [Header("Leader")]
    public List<Enum_HeroClass_Tags> tags = new();
    public LeaderTraits leaderTraits;
    [Header("Promotions")]
    public HeroClass promotion;
    [Header("Name Generation")]
    public PrefixType prefix;
    public SuffixType suffix;

    static readonly List<HeroClass> allHeroClasses = new();
    public static HeroClass[] All 
    { 
        get 
        { 
            if(allHeroClasses.Count == 0)
            {
                HeroClass[] loadedClasses = Resources.LoadAll<HeroClass>("Classes");

                if (loadedClasses != null && loadedClasses.Length > 0)
                {
                    allHeroClasses.AddRange(loadedClasses);
                    Debug.Log($"Loaded {allHeroClasses.Count} HeroClass ScriptableObjects.");
                }
                else
                {
                    Debug.LogWarning("No HeroClass ScriptableObjects found in Resources/Classes.");
                }
            }

            return allHeroClasses.ToArray(); 
        } 
    }
    public static HeroClass GetClass(int ID)
    {
        foreach(var item in allHeroClasses)
        {
            string sub = item.name[6..];
            if (sub.Contains(ID.ToString("000")))
                return item;
        }

        return null;
    }
     
    public Attributes GenerateStats(int level)
    {
        Attributes stats = new()
        {
            health = baseStats.health + statGrowth.health * level,
            mana = baseStats.mana + statGrowth.mana * level,
            power = baseStats.power + statGrowth.power * level,
            armor = baseStats.armor + statGrowth.armor * level,
            magic = baseStats.magic + statGrowth.magic * level,
            resist = baseStats.resist + statGrowth.resist * level,
            speed = baseStats.speed + statGrowth.speed * level,
            accuracy = baseStats.accuracy + statGrowth.accuracy * level,
            evasion = baseStats.evasion + statGrowth.evasion * level,
            shield = baseStats.shield + statGrowth.shield * level,
            actions = baseStats.actions + statGrowth.actions * level,
            reactions = baseStats.reactions + statGrowth.reactions * level,
            potential = RollPotential(),
        };

        return stats;
    }
    public float RollPotential()
    {
        float total = 0;
        for (int i = 0; i < 5; i++)
            total += Random.Range(1, 25);

        return total;
    }
    public bool CanUseEquipment(Equipment item)
    {
        if (item && usableEquipment.Count > 0)
            return usableEquipment.Contains(item.equipmentType);

        return true;
    }
    public BattleMove[] KnownSkills(int level)
    {
        List<BattleMove> result = new();

        foreach(var item in skills)
        {
            if (item.move != null && item.level <= level)
                result.Add(item.move);
        }

        return result.ToArray();
    }
    public string GetPromotionRequirement()
    {
        int cost = BattleConstants.GetHeroPromotionCost(this);
        if (cost < 0) return "[max rank]";
        return $"{cost} Gold";
    }
    public bool HasPromotionRequirement()
    {
        int cost = BattleConstants.GetHeroPromotionCost(this);
        return cost >= 0 && GameManager.Inventory.CanSpend(cost);
    }
    public void UsePromotionRequirement()
    {
        int cost = BattleConstants.GetHeroPromotionCost(this);
        if (cost >= 0)  GameManager.Inventory.SpendMoney(cost);
    }

    public Color RarityColor()
    {
        return BattleConstants.GetBadgeColor(rarity);
    } 
    public Color RankColor()
    {
        return BattleConstants.GetBadgeColor(rarity);
    }
    public string GenerateRandomName()
    {
        return RandomNameGenerator.GenerateName(prefix, suffix);
    }
}

[System.Serializable]
public class LearnableSkill
{
    [Range(1, 20)]
    public int level = 1;
    public BattleMove move; 
}
