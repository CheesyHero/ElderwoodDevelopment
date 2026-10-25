using UnityEngine; 
using System.Collections.Generic;
using System.Linq;

public enum FilterRuleType
{
    LowestCurrentHealth = 0,
    HighestCurrentHealth = 1,
    LowestHealthPercent = 2,
    HighestHealthPercent = 3,

    HighestShieldValue = 4,
    LowestShieldValue = 5,
    HighestManaValue = 6,
    LowestManaValue = 7,

    OverHealthPercent = 10,
    UnderHealthPercent = 11,

    ActionPointsGreaterThan = 20,
    ActionPointsLesserThan = 21,
    ActionPointsHighest = 22,
    ActionPointsLowest = 23,

    ReactionPointsGreaterThan = 25,
    ReactionPointsLesserThan = 26,
    ReactionPointsHighest = 27,
    ReactionPointsLowest = 28,

    HighestAttribute = 100,
    LowestAttribute = 101,

    Self = 190,
    Other = 191,

    OwnerOverHealthPercent = 200,
    OwnerUnderHealthPercent = 201,

    OwnerActionPointsGreaterThan = 210,
    OwnerActionPointsLesserThan = 211,

    OwnerReactionPointsGreaterThan = 220,
    OwnerReactionPointsLesserThan = 221,

    IsLeader = 290,
    IsNotLeader = 291,
    IncompleteRow = 299,
    FullRow = 300,
    RowWithEqualOrGreater = 301,
    RowWithEqualOrLesser = 302,

    PrioritizeFrontRow = 303,
    PrioritizeBackRow = 304,
    FrontRowOnly = 305,
    BackRowOnly = 306,

    PrioritizeRow = 307,
    RowOnly = 308,

    EnemySurvivors_GreaterOrEqual = 350,
    EnemySurvivors_LessOrEqual = 351,
    AlliedSurvivors_GreaterOrEqual = 352,
    AlliedSurvivors_LessOrEqual = 353,

   ContainsStatusCondition = 400,
   LacksStatusCondition = 401,
}
public enum E_MoveFilterMode
{
    picker = 0, // Picks only one item from a list
    filter = 1, // Pulls all valid items from a list
}

[System.Serializable]
public class FilterRule
{
    public FilterRuleType type;
    public E_MoveFilterMode mode;

    [Tooltip("For Enumarable Values\n" +
        "\r\n    health = 0,\r\n    mana = 1, \r\n    power = 2, \r\n    " +
        "armor = 3, \r\n    magic = 4, \r\n    resist = 5, \r\n    speed = 6, " +
        "\r\n    accuracy = 7, \r\n    evasion = 8, \r\n    shield = 9,\r\n    " +
        "actions = 10,\r\n    reactions = 11,\r\n" +
        "\r\n    row values: Front = 0, Back = 1")]
    public float value;

    public BattleUnit GetParentUnit(BattleCombatant hero) => hero != null ? hero.parent : null;

    public List<BattleCombatant> Filter(BattleCombatant owner, List<BattleCombatant> list)
    {
        if (list == null || list.Count == 0) return list;

        return type switch
        {
            FilterRuleType.OwnerOverHealthPercent =>
                (owner != null && HealthPercent(owner) > Mathf.Clamp01(value))
                    ? list : new List<BattleCombatant>(0),

            FilterRuleType.OwnerUnderHealthPercent =>
                (owner != null && HealthPercent(owner) < Mathf.Clamp01(value))
                    ? list : new List<BattleCombatant>(0),

            FilterRuleType.OwnerActionPointsGreaterThan =>
                (owner != null && ActionPoints(owner) >= Mathf.RoundToInt(value))
                    ? list : new List<BattleCombatant>(0),

            FilterRuleType.OwnerActionPointsLesserThan =>
                (owner != null && ActionPoints(owner) <= Mathf.RoundToInt(value))
                    ? list : new List<BattleCombatant>(0),

            FilterRuleType.OwnerReactionPointsGreaterThan =>
                (owner != null && ReactionPoints(owner) >= Mathf.RoundToInt(value))
                    ? list : new List<BattleCombatant>(0),

            FilterRuleType.OwnerReactionPointsLesserThan =>
                (owner != null && ReactionPoints(owner) <= Mathf.RoundToInt(value))
                    ? list : new List<BattleCombatant>(0),

            FilterRuleType.EnemySurvivors_GreaterOrEqual =>
                (owner != null && CountLivingEnemies(owner) >= Mathf.RoundToInt(value))
                    ? list : new List<BattleCombatant>(0),

            FilterRuleType.EnemySurvivors_LessOrEqual =>
                (owner != null && CountLivingEnemies(owner) <= Mathf.RoundToInt(value))
                    ? list : new List<BattleCombatant>(0),

            FilterRuleType.HighestCurrentHealth =>
                FilterHighestCurrentHP(list),

            FilterRuleType.LowestCurrentHealth =>
                FilterLowestCurrentHP(list),

            FilterRuleType.HighestHealthPercent =>
                FilterHighestPercentHP(list),

            FilterRuleType.LowestHealthPercent =>
                FilterLowestPercentHP(list),

            FilterRuleType.HighestShieldValue =>
                FilterHighestShield(list),

            FilterRuleType.LowestShieldValue =>
                FilterLowestShield(list),

            FilterRuleType.HighestManaValue =>
                FilterHighestMana(list),

            FilterRuleType.LowestManaValue =>
                FilterLowestMana(list),

            FilterRuleType.OverHealthPercent =>
                FilterOverPercentHP(list, value),

            FilterRuleType.UnderHealthPercent =>
                FilterUnderPercentHP(list, value),

            FilterRuleType.HighestAttribute =>
                FilterHighestAttribute(list, (Enum_HeroAttributes)Mathf.RoundToInt(value)),

            FilterRuleType.LowestAttribute =>
                FilterLowestAttribute(list, (Enum_HeroAttributes)Mathf.RoundToInt(value)),

            FilterRuleType.FullRow =>
                FilterFullRow(list),

            FilterRuleType.IncompleteRow =>
                FilterIncompleteRow(list),

            FilterRuleType.RowWithEqualOrGreater =>
                FilterRowWithEqualOrGreater(list, Mathf.RoundToInt(value)),

            FilterRuleType.RowWithEqualOrLesser =>
                FilterRowWithEqualOrLesser(list, Mathf.RoundToInt(value)),

            FilterRuleType.PrioritizeFrontRow =>
                PrioritizeRowIfAvailable(list, true),

            FilterRuleType.PrioritizeBackRow =>
                PrioritizeRowIfAvailable(list, false),

            FilterRuleType.FrontRowOnly =>
                FilterRowOnly(list, true),

            FilterRuleType.BackRowOnly =>
                FilterRowOnly(list, false),

            FilterRuleType.PrioritizeRow =>
                PrioritizeRowIfAvailable(list, IsFrontRowValue()),

            FilterRuleType.RowOnly =>
                FilterRowOnly(list, IsFrontRowValue()),

            // Self targeting
            FilterRuleType.Self =>
                owner != null && list.Contains(owner)
                    ? new List<BattleCombatant>() { owner }
                    : new List<BattleCombatant>(),

            FilterRuleType.Other =>
                FilterOther(owner, list),

            // Allied survivor conditions
            FilterRuleType.AlliedSurvivors_GreaterOrEqual =>
                (owner != null &&
                 CountLivingAllies(owner) >= Mathf.RoundToInt(value))
                    ? list : new List<BattleCombatant>(),

            FilterRuleType.AlliedSurvivors_LessOrEqual =>
                (owner != null &&
                 CountLivingAllies(owner) <= Mathf.RoundToInt(value))
                    ? list : new List<BattleCombatant>(),

            // Status condition targeting
            FilterRuleType.ContainsStatusCondition =>
                FilterByStatusCondition(list, Mathf.RoundToInt(value), true),

            FilterRuleType.LacksStatusCondition =>
                FilterByStatusCondition(list, Mathf.RoundToInt(value), false),

            FilterRuleType.ActionPointsHighest =>
                FilterHighestAP(list),

            FilterRuleType.ActionPointsLowest =>
                FilterLowestAP(list),

            FilterRuleType.ReactionPointsHighest =>
                FilterHighestRP(list),

            FilterRuleType.ReactionPointsLowest =>
                FilterLowestRP(list),

            FilterRuleType.IsLeader =>
                FilterByLeader(list, true),

            FilterRuleType.IsNotLeader =>
                FilterByLeader(list, false),

            _ => list,
        };
    }

    public string PrintEffectDetails()
    {
        string v = value.ToString("0.##");

        return type switch
        {
            FilterRuleType.OwnerOverHealthPercent =>
                $"User {Attributes.PrintAttributeName(Enum_HeroAttributes.health)} above <color=#f03040>{value * 100f}%</color>",

            FilterRuleType.OwnerUnderHealthPercent =>
                $"User {Attributes.PrintAttributeName(Enum_HeroAttributes.health)} below <color=#51cbf0>{value * 100f}%</color>",

            FilterRuleType.OwnerActionPointsGreaterThan =>
                $"User {Attributes.PrintAttributeName(Enum_HeroAttributes.actions)} <u>greater</u> than or equal to <color=#ffa721><u><b>{v}</b></u></color>",

            FilterRuleType.OwnerActionPointsLesserThan =>
                $"User {Attributes.PrintAttributeName(Enum_HeroAttributes.actions)} <u>less</u> than or equal to <color=#ffa721><u><b>{v}</b></u></color>",

            FilterRuleType.OwnerReactionPointsGreaterThan =>
                $"User {Attributes.PrintAttributeName(Enum_HeroAttributes.reactions)} <u>greater</u> than or equal to <color=#ffa721><u><b>{v}</b></u></color>",

            FilterRuleType.OwnerReactionPointsLesserThan =>
                $"User {Attributes.PrintAttributeName(Enum_HeroAttributes.reactions)} <u>less</u> than or equal to <color=#ffa721><u><b>{v}</b></u></color>",

            FilterRuleType.HighestCurrentHealth =>
                $"<color=#fce37e>Highest</color> current {Attributes.PrintAttributeName(Enum_HeroAttributes.health)}",

            FilterRuleType.LowestCurrentHealth =>
                $"<color=#d679ed>Lowest</color> current {Attributes.PrintAttributeName(Enum_HeroAttributes.health)}",

            FilterRuleType.HighestHealthPercent =>
                $"<color=#fce37e>Highest</color> {Attributes.PrintAttributeName(Enum_HeroAttributes.health)} percent",

            FilterRuleType.LowestHealthPercent =>
                $"<color=#d679ed>Lowest</color> {Attributes.PrintAttributeName(Enum_HeroAttributes.health)} percent",

            FilterRuleType.HighestShieldValue =>
                $"<color=#fce37e>Highest</color> {Attributes.PrintAttributeName(Enum_HeroAttributes.shield)} value",

            FilterRuleType.LowestShieldValue =>
                $"<color=#d679ed>Lowest</color> {Attributes.PrintAttributeName(Enum_HeroAttributes.shield)} value",

            FilterRuleType.HighestManaValue =>
                $"<color=#fce37e>Highest</color> {Attributes.PrintAttributeName(Enum_HeroAttributes.mana)} value",

            FilterRuleType.LowestManaValue =>
                $"<color=#d679ed>Lowest</color> {Attributes.PrintAttributeName(Enum_HeroAttributes.mana)} value",

            FilterRuleType.OverHealthPercent =>
                $"<color=#fce37e>Above</color> {value * 100f}% {Attributes.PrintAttributeName(Enum_HeroAttributes.health)}",

            FilterRuleType.UnderHealthPercent =>
                $"<color=#d679ed>Below</color> {value * 100f}% {Attributes.PrintAttributeName(Enum_HeroAttributes.health)}",

            FilterRuleType.HighestAttribute =>
                $"<color=#fce37e>Highest</color> total {Attributes.PrintAttributeName((Enum_HeroAttributes)Mathf.RoundToInt(value))}",

            FilterRuleType.LowestAttribute =>
                $"<color=#d679ed>Lowest</color> total {Attributes.PrintAttributeName((Enum_HeroAttributes)Mathf.RoundToInt(value))}",

            FilterRuleType.ActionPointsGreaterThan =>
                $"{Attributes.PrintAttributeName(Enum_HeroAttributes.actions)} <color=#fce37e>greater</color> than or equal to <color=#ffa721><u><b>{v}</b></u></color>",

            FilterRuleType.ActionPointsLesserThan =>
                $"{Attributes.PrintAttributeName(Enum_HeroAttributes.actions)} <color=#d679ed>less</color> than or equal to <color=#ffa721><u><b>{v}</b></u></color>",

            FilterRuleType.ReactionPointsGreaterThan =>
                $"{Attributes.PrintAttributeName(Enum_HeroAttributes.reactions)} <color=#fce37e>greater</color> than or equal to <color=#95edf0><u><b>{v}</b></u></color>",

            FilterRuleType.ReactionPointsLesserThan =>
                $"{Attributes.PrintAttributeName(Enum_HeroAttributes.reactions)} <color=#d679ed>less</color> than or equal to <color=#95edf0><u><b>{v}</b></u></color>",

            FilterRuleType.FullRow => "Targets in a <color=#fce37e>full row</color>",

            FilterRuleType.IncompleteRow => "Targets in an <color=#d679ed>incomplete row</color>",

            FilterRuleType.RowWithEqualOrGreater =>
                $"Targets in row with <color=#fce37e>{v}</color> or more heroes",

            FilterRuleType.RowWithEqualOrLesser =>
                $"Targets in row with <color=#d679ed>{v}</color> or fewer heroes",

            FilterRuleType.PrioritizeFrontRow =>
                "Prioritize <color=#fce37e>front row</color> targets",

            FilterRuleType.PrioritizeBackRow =>
                "Prioritize <color=#fce37e>back row</color> targets",

            FilterRuleType.FrontRowOnly =>
                "<color=#fce37e>Front row only</color>",

            FilterRuleType.BackRowOnly =>
                "<color=#fce37e>Back row only</color>",

            FilterRuleType.PrioritizeRow =>
                $"Prioritize {(IsFrontRowValue() ? "<color=#fce37e>front</color>" : "<color=#fce37e>back</color>")} row targets",

            FilterRuleType.RowOnly =>
                $"{(IsFrontRowValue() ? "<color=#fce37e>Front</color>" : "<color=#fce37e>Back</color>")} row only",

            FilterRuleType.EnemySurvivors_GreaterOrEqual =>
                $"Enemy Heroes <color=#fce37e>greater</color> than or equal to <color=#ffa721><u><b>{v}</b></u></color>",

            FilterRuleType.EnemySurvivors_LessOrEqual =>
                $"Enemy Heroes <color=#d679ed>less</color> than or equal to <color=#ffa721><u><b>{v}</b></u></color>",

            FilterRuleType.Self => "Prioritize <color=#fce37e>self</color>",

            FilterRuleType.Other => "Target <color=#fce37e>anyone except self</color>",

            FilterRuleType.AlliedSurvivors_GreaterOrEqual =>
                $"Allied survivors <color=#fce37e>greater</color> than or equal to <color=#ffa721><b>{v}</b></color>",

            FilterRuleType.AlliedSurvivors_LessOrEqual =>
                $"Allied survivors <color=#d679ed>less</color> than or equal to <color=#ffa721><b>{v}</b></color>",

            FilterRuleType.ContainsStatusCondition =>
                PrintStatusCondition(true),

            FilterRuleType.LacksStatusCondition =>
                PrintStatusCondition(false),

            FilterRuleType.ActionPointsHighest =>
                $"<color=#fce37e>Highest</color> current {Attributes.PrintAttributeName(Enum_HeroAttributes.actions)}",

            FilterRuleType.ActionPointsLowest =>
                $"<color=#d679ed>Lowest</color> current {Attributes.PrintAttributeName(Enum_HeroAttributes.actions)}",

            FilterRuleType.ReactionPointsHighest =>
                $"<color=#fce37e>Highest</color> current {Attributes.PrintAttributeName(Enum_HeroAttributes.reactions)}",

            FilterRuleType.ReactionPointsLowest =>
                $"<color=#d679ed>Lowest</color> current {Attributes.PrintAttributeName(Enum_HeroAttributes.reactions)}",

            FilterRuleType.IsLeader => "Target <color=#fce37e>unit leaders</color>",

            FilterRuleType.IsNotLeader =>  "Target <color=#d679ed>non-leader heroes</color>",

            _ =>
                "Unknown filter rule",
        };
    }

    float CurrentHP(BattleCombatant c)
        => (c != null && c.hero != null && c.hero.health != null) ? c.hero.health.health_current : 0f;

    float MaxHP(BattleCombatant c)
        => (c != null && c.hero != null && c.hero.health != null) ? Mathf.Max(1f, c.hero.health.health_max) : 1f;

    float HealthPercent(BattleCombatant c)
        => Mathf.Clamp01(CurrentHP(c) / MaxHP(c));

    float CurrentShield(BattleCombatant c)
        => (c != null && c.hero != null && c.hero.health != null) ? c.hero.health.shield_current : 0f;

    float CurrentMana(BattleCombatant c)
        => (c != null && c.hero != null && c.hero.health != null) ? c.hero.health.mana_current : 0f;

    int ActionPoints(BattleCombatant c)
        => (c != null) ? c.actionPoints : 0;

    int ReactionPoints(BattleCombatant c)
        => (c != null) ? c.reactionPoints : 0;

    bool IsFrontRowValue() => Mathf.RoundToInt(value) <= 0;

    int CountLivingEnemies(BattleCombatant owner)
    {
        if (owner == null) return 0;

        int total = 0;
        var enemies = owner.GetAllEnemies();
        foreach (var enemy in enemies)
        {
            if (enemy != null && enemy.IsAlive)
                total++;
        }

        return total;
    }

    List<BattleCombatant> FilterOverPercentHP(List<BattleCombatant> list, float threshold)
        => list.Where(c => c != null && HealthPercent(c) > Mathf.Clamp01(threshold)).ToList();

    List<BattleCombatant> FilterUnderPercentHP(List<BattleCombatant> list, float threshold)
        => list.Where(c => c != null && HealthPercent(c) < Mathf.Clamp01(threshold)).ToList();

    public List<BattleCombatant> FilterHighestCurrentHP(List<BattleCombatant> list)
    {
        float best = list.Where(c => c != null).Select(CurrentHP).DefaultIfEmpty(0f).Max();
        return list.Where(c => c != null && Mathf.Approximately(CurrentHP(c), best)).ToList();
    }

    public List<BattleCombatant> FilterLowestCurrentHP(List<BattleCombatant> list)
    {
        float best = list.Where(c => c != null).Select(CurrentHP).DefaultIfEmpty(0f).Min();
        return list.Where(c => c != null && Mathf.Approximately(CurrentHP(c), best)).ToList();
    }

    public List<BattleCombatant> FilterHighestPercentHP(List<BattleCombatant> list)
    {
        float best = list.Where(c => c != null).Select(HealthPercent).DefaultIfEmpty(0f).Max();
        return list.Where(c => c != null && Mathf.Approximately(HealthPercent(c), best)).ToList();
    }

    public List<BattleCombatant> FilterLowestPercentHP(List<BattleCombatant> list)
    {
        float best = list.Where(c => c != null).Select(HealthPercent).DefaultIfEmpty(0f).Min();
        return list.Where(c => c != null && Mathf.Approximately(HealthPercent(c), best)).ToList();
    }

    public List<BattleCombatant> FilterHighestShield(List<BattleCombatant> list)
    {
        float best = list.Where(c => c != null).Select(CurrentShield).DefaultIfEmpty(0f).Max();
        return list.Where(c => c != null && Mathf.Approximately(CurrentShield(c), best)).ToList();
    }

    public List<BattleCombatant> FilterLowestShield(List<BattleCombatant> list)
    {
        float best = list.Where(c => c != null).Select(CurrentShield).DefaultIfEmpty(0f).Min();
        return list.Where(c => c != null && Mathf.Approximately(CurrentShield(c), best)).ToList();
    }

    public List<BattleCombatant> FilterHighestMana(List<BattleCombatant> list)
    {
        float best = list.Where(c => c != null).Select(CurrentMana).DefaultIfEmpty(0f).Max();
        return list.Where(c => c != null && Mathf.Approximately(CurrentMana(c), best)).ToList();
    }

    public List<BattleCombatant> FilterLowestMana(List<BattleCombatant> list)
    {
        float best = list.Where(c => c != null).Select(CurrentMana).DefaultIfEmpty(0f).Min();
        return list.Where(c => c != null && Mathf.Approximately(CurrentMana(c), best)).ToList();
    }

    public List<BattleCombatant> FilterFullRow(List<BattleCombatant> list)
    {
        if (list == null || list.Count == 0) return list;

        BattleUnit unit = GetParentUnit(list[0]);
        if (unit == null) return list;

        var fullRow = unit.GetFullRow();
        if (fullRow == null || fullRow.Count == 0)
            return new List<BattleCombatant>(0);

        return list.Where(c => c != null && fullRow.Contains(c)).ToList();
    }

    public List<BattleCombatant> FilterRowWithEqualOrGreater(List<BattleCombatant> list, int targetCount)
    {
        if (list == null || list.Count == 0) return list;

        BattleUnit unit = GetParentUnit(list[0]);
        if (unit == null) return list;

        var front = unit.GetFirstRow();
        var back = unit.GetBackRow();

        bool frontValid = front.Count >= targetCount;
        bool backValid = back.Count >= targetCount;

        if (!frontValid && !backValid)
            return new List<BattleCombatant>(0);

        List<BattleCombatant> chosen =
            frontValid && backValid ? (front.Count >= back.Count ? front : back) :
            frontValid ? front :
            back;

        return list.Where(c => c != null && chosen.Contains(c)).ToList();
    }

    public List<BattleCombatant> FilterRowWithEqualOrLesser(List<BattleCombatant> list, int targetCount)
    {
        if (list == null || list.Count == 0) return list;

        BattleUnit unit = GetParentUnit(list[0]);
        if (unit == null) return list;

        var front = unit.GetFirstRow();
        var back = unit.GetBackRow();

        bool frontValid = front.Count <= targetCount && front.Count > 0;
        bool backValid = back.Count <= targetCount && back.Count > 0;

        if (!frontValid && !backValid)
            return new List<BattleCombatant>(0);

        List<BattleCombatant> chosen =
            frontValid && backValid ? (front.Count <= back.Count ? front : back) :
            frontValid ? front :
            back;

        return list.Where(c => c != null && chosen.Contains(c)).ToList();
    }

    public List<BattleCombatant> PrioritizeRowIfAvailable(List<BattleCombatant> list, bool front)
    {
        if (list == null || list.Count == 0) return list;

        var filtered = list.Where(c => c != null && (front ? c.IsFrontRow() : c.IsBackRow())).ToList();
        return filtered.Count > 0 ? filtered : list;
    }

    public List<BattleCombatant> FilterRowOnly(List<BattleCombatant> list, bool front)
    {
        if (list == null || list.Count == 0) return list;

        return list.Where(c => c != null && (front ? c.IsFrontRow() : c.IsBackRow())).ToList();
    }

    float GetAttributeValue(BattleCombatant c, Enum_HeroAttributes attribute)
    {
        if (c == null || c.hero == null) return 0f;

        Attributes a = c.Attributes;

        return attribute switch
        {
            Enum_HeroAttributes.health => a.health,
            Enum_HeroAttributes.mana => a.mana,
            Enum_HeroAttributes.power => a.power,
            Enum_HeroAttributes.armor => a.armor,
            Enum_HeroAttributes.magic => a.magic,
            Enum_HeroAttributes.resist => a.resist,
            Enum_HeroAttributes.speed => a.speed,
            Enum_HeroAttributes.accuracy => a.accuracy,
            Enum_HeroAttributes.evasion => a.evasion,
            Enum_HeroAttributes.shield => a.shield,
            Enum_HeroAttributes.actions => a.actions,
            Enum_HeroAttributes.reactions => a.reactions,
            _ => 0f,
        };
    }

    public List<BattleCombatant> FilterHighestAttribute(List<BattleCombatant> list, Enum_HeroAttributes attribute)
    {
        float best = list.Where(c => c != null).Select(c => GetAttributeValue(c, attribute)).DefaultIfEmpty(0f).Max();
        return list.Where(c => c != null && Mathf.Approximately(GetAttributeValue(c, attribute), best)).ToList();
    }

    public List<BattleCombatant> FilterLowestAttribute(List<BattleCombatant> list, Enum_HeroAttributes attribute)
    {
        float best = list.Where(c => c != null).Select(c => GetAttributeValue(c, attribute)).DefaultIfEmpty(0f).Min();
        return list.Where(c => c != null && Mathf.Approximately(GetAttributeValue(c, attribute), best)).ToList();
    }

    int CountLivingAllies(BattleCombatant owner)
    {
        if (owner == null) return 0;

        int total = 0;

        var allies = owner.GetAllAllies();

        foreach (var ally in allies)
        {
            if (ally != null && ally.IsAlive)
                total++;
        }

        return total;
    }

    bool HasStatusCondition(BattleCombatant combatant, int statusID)
    {
        if (combatant == null)
            return false;

        foreach (var condition in combatant.StatusConditions)
        {
            if (condition == null || condition.status == null)
                continue;

            if (condition.status.GetID() == statusID)
                return true;
        }

        return false;
    }

    List<BattleCombatant> FilterByStatusCondition(List<BattleCombatant> list,  int statusID, bool contains)
    {
        return list.Where(c =>
            c != null &&
            HasStatusCondition(c, statusID) == contains
        ).ToList();
    }
    public List<BattleCombatant> FilterHighestAP(List<BattleCombatant> list)
    {
        int best = list.Where(c => c != null)
            .Select(ActionPoints)
            .DefaultIfEmpty(0)
            .Max();

        return list.Where(c => c != null && ActionPoints(c) == best).ToList();
    }

    public List<BattleCombatant> FilterLowestAP(List<BattleCombatant> list)
    {
        int best = list.Where(c => c != null)
            .Select(ActionPoints)
            .DefaultIfEmpty(0)
            .Min();

        return list.Where(c => c != null && ActionPoints(c) == best).ToList();
    } 
    public List<BattleCombatant> FilterHighestRP(List<BattleCombatant> list)
    {
        int best = list.Where(c => c != null)
            .Select(ReactionPoints)
            .DefaultIfEmpty(0)
            .Max();

        return list.Where(c => c != null && ReactionPoints(c) == best).ToList();
    }

    public List<BattleCombatant> FilterLowestRP(List<BattleCombatant> list)
    {
        int best = list.Where(c => c != null)
            .Select(ReactionPoints)
            .DefaultIfEmpty(0)
            .Min();

        return list.Where(c => c != null && ReactionPoints(c) == best).ToList();
    } 
    public List<BattleCombatant> FilterOther(BattleCombatant owner, List<BattleCombatant> list)
    {
        return list.Where(c => c != null && c != owner).ToList();
    }

    public List<BattleCombatant> FilterIncompleteRow(List<BattleCombatant> list)
    {
        if (list == null || list.Count == 0)
            return list;

        // Each row has 3 slots.
        const int maxRowSize = 3;

        return list.Where(c =>
        {
            if (c == null || c.parent == null)
                return false;

            BattleUnit unit = c.parent;

            int rowCount = c.IsFrontRow()
                ? unit.GetFirstRow().Count
                : unit.GetBackRow().Count;

            return rowCount < maxRowSize;

        }).ToList();
    } 
    public List<BattleCombatant> FilterByLeader(List<BattleCombatant> list, bool isLeader)
    {
        if (list == null || list.Count == 0)
            return new();

        return list.Where(c =>
            c != null &&
            c.parent != null &&
            (c.parent.parent.Leader() == c.hero) == isLeader
        ).ToList();
    }

    private string PrintStatusCondition(bool contains)
    {
        int id = Mathf.RoundToInt(value);
        Passive status = Passive.Get(id);

        string statusName = status != null
            ? status.skillName
            : $"Unknown Status ({id})";

        string statusColor = status != null
            ? ColorUtility.ToHtmlStringRGB(status.color)
            : "FFFFFF";

        string label = contains
            ? "<color=#fce37e>Has</color>"
            : "<color=#d679ed>Missing</color>";

        return $"{label} <color=#{statusColor}><b>{statusName}</b></color> status";
    }
}