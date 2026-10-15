using UnityEngine;

public static class BattleConstants 
{
    public static readonly int maxLevel = 20;
    public static readonly float EXP_BaseRequirement = 20f; // base +
    public static readonly float EXP_ExponentValue = 3f; // level ^ exponent 
    public static readonly float EXPCurve = 0.3f;
     
    public static readonly Color commonColor = Color.gray2;
    public static readonly Color uncommonColor = Color.cadetBlue;
    public static readonly Color rareColor = Color.gold;
    public static readonly Color mythicColor = Color.red;
    public static readonly Color legendaryColor = Color.purple;

    public static int GetEXPRequirement(int level)
    {
        level = Mathf.Clamp(level, 1, maxLevel);
        return Mathf.RoundToInt(EXP_BaseRequirement * level + Mathf.Pow(level, EXP_ExponentValue));
    } 
    public static string WrapWithBadgeColor(string input, RarityOptions rarity)
    { 
        string hex = ColorUtility.ToHtmlStringRGB(GetBadgeColor(rarity));
        return $"<color=#{hex}>{RandomNameGenerator.Capitalize(input)}</color>";
    }
    public static Color GetBadgeColor(RarityOptions options)
    {
        return options switch
        {
            RarityOptions.Uncommon => uncommonColor,
            RarityOptions.Rare => rareColor,
            RarityOptions.Mythic => mythicColor,
            RarityOptions.Legendary => legendaryColor,
            _ => commonColor,
        };
    }
    public static Color GetBadgeColor(int rank)
    {
        return rank switch
        {
            1 => uncommonColor,
            2 => rareColor,
            3 => mythicColor,
            4 => legendaryColor,
            _ => commonColor,
        };
    }
    public static int GetHeroHireCost(HeroClass hero)
    {
        return hero.rarity switch
        {
            RarityOptions.Uncommon => 500,
            RarityOptions.Rare => 800,
            RarityOptions.Mythic => 1200,
            RarityOptions.Legendary => 2000,
            // Default to common
            _ => 300,
        };
    }

    public static int GetAddUnitCost(Army army)
    {
        int count = army.units.Count;

        if (count <= 3) return 100;

        return count switch
        {
            4 => 200,
            5 => 400,
            6 => 600,
            7 => 800,
            _ => -1,
        };
    }
    public static int GetUnitSizeUpgradeCost(Unit unit)
    {
        return unit.maxPlacements switch
        {
            0 => 0,
            1 => 0,
            2 => 300,
            3 => 1000,
            4 => 2000,
            _ => -1,
        };
    }
    public static int GetHeroPromotionCost(Hero hero) => GetHeroPromotionCost(hero.heroClass);
    public static int GetHeroPromotionCost(HeroClass hero)
    {
        if (hero == null) return -1;

        return hero.rank switch
        {
            1 => 600,
            2 => 1200,
            _ => -1,
        };
    }
    public static string PrintMoveTypeWithColor(BattleMove move)
    {
        MoveType t = move.moveType;
        string _name = RandomNameGenerator.Capitalize(move.damageType.ToString()) + 
            " " + RandomNameGenerator.Capitalize(t.ToString());
        return t switch
        {
            MoveType.action => $"<color=#ed4f3e>{_name}</color>",
            MoveType.reaction => $"<color=#3ea4ed>{_name}</color>",
            MoveType.special => $"<color=#8d3eed>{_name}</color>",
            _ => _name,
        };
    }
}
