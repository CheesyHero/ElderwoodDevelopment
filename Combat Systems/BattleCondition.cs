using System.Collections.Generic;
using UnityEngine; 

public enum Enum_BattleCondition_Targets
{
    owner = 0, source = 1, target = 2, self = 3, 
    prefered = 4, all = 5, allies = 6, enemies = 7,
}
public enum Enum_BattleCondition_Conditions
{
    randomChance = 0,
    healthPercent = 1, manaPercent = 2,
    healthValue = 6, shieldValue = 7, manaValue = 8, 
    isAllied = 9, isOpponent = 10, isSelf = 11,
    isAlive = 12,
    isDefeated = 13, 
    hasStatusConditionID = 15, 
    actionPoints = 16,
    reactionPoints = 17, 

    survivingAllies = 18,
    survivingEnemies = 19,

    moveIsPhysical = 20,
    moveIsMagical = 21,
    moveContainsDamage = 22,
    moveIsMelee = 23,
    moveContainsDebuff = 24,
    moveContainsStatusCondition = 25,
    moveHitsAoE = 26,
    moveIsAction = 27,
    moveIsReaction = 28,
    moveIsSpecial = 29,
    moveIsActionOrSpecial = 30,

    isLeader = 35,

    actionsUsed = 40, 
    reactionsUsed = 41, 
    totalActionsUsed = 42,

    tileType = 50,
}
public enum Enum_BattleCondition_Comparison
{
    equalTo = 0, greaterThan = 1, greaterThenOrEqualTo = 2,
    lesserThan = 3, lesserThanOrEqualTo = 4, notEqual = 5,
}
[System.Serializable]
public class BattleCondition 
{
    [Tooltip("This is only for the instance of multiple targets.\n" +
        "When evaluating multiple targets, this setting states that ALL targets must return TRUE.\n" +
        "Otherwise only ONE target must return TRUE.")] 
    public Enum_BattleCondition_Targets target;
    public Enum_BattleCondition_Conditions condition;
    public Enum_BattleCondition_Comparison comparison; 
    [Tooltip("The value we want to compare against\n" +
        "\nplains = 0\n road = 1\n forest = 2\n mountain = 3\n water = 4" +
        "\n empty = 5\n stronghold = 20\n village = 21\n wall = 22")]
    public float value; // The value we want to check
    public bool RunCondition(BattleCombatant checker, BattleEvent battleEvent)
    {
        if (battleEvent == null) return false; 
        return Parse(checker, battleEvent);
    }
    protected bool Parse(BattleCombatant checker, BattleEvent battleEvent)
    { 
        if (target == Enum_BattleCondition_Targets.self)
            return Evaluate(checker, checker, battleEvent);

        List<BattleCombatant> targets = GetTarget(battleEvent);
        if (targets == null || targets.Count == 0)
            return false;

        foreach (var item in targets)
        {
            if (item == null) return false; 
            if (!Evaluate(checker, item, battleEvent)) return false;

        }
        return true;
    }


    private List<BattleCombatant> GetTarget(BattleEvent battleEvent)
    {
        if (battleEvent == null) return new();

        return target switch
        {
            Enum_BattleCondition_Targets.source =>
                battleEvent.source != null ? new List<BattleCombatant> { battleEvent.source } : new(),

            Enum_BattleCondition_Targets.target =>
                battleEvent.targets ?? new(), 

            _ =>
                battleEvent.owner != null ? new List<BattleCombatant> { battleEvent.owner } : new(),
        };
    }
    protected bool Evaluate(BattleCombatant checker, BattleCombatant target, BattleEvent battleEvent)
    {
        if (target == null) return false;

        BattleMove move = battleEvent?.move;

        return condition switch
        {
            // --- existing ---
            Enum_BattleCondition_Conditions.randomChance =>
                Calculate(UnityEngine.Random.Range(1, 100)),

            Enum_BattleCondition_Conditions.healthPercent =>
                Calculate(target.hero.health.HealthPercent),

            Enum_BattleCondition_Conditions.manaPercent =>
                Calculate(target.hero.health.ManaPercent),

            Enum_BattleCondition_Conditions.healthValue =>
                Calculate(target.hero.health.health_current),

            Enum_BattleCondition_Conditions.shieldValue =>
                Calculate(target.hero.health.shield_current),

            Enum_BattleCondition_Conditions.manaValue =>
                Calculate(target.hero.health.mana_current),

            Enum_BattleCondition_Conditions.actionPoints =>
                Calculate(target != null ? target.actionPoints : 0),

            Enum_BattleCondition_Conditions.reactionPoints =>
                Calculate(target != null ? target.reactionPoints : 0),

            Enum_BattleCondition_Conditions.actionsUsed =>
                Calculate(target != null ? target.actionsUsed : 0),

            Enum_BattleCondition_Conditions.reactionsUsed =>
                Calculate(target != null ? target.reactionsUsed : 0),

            Enum_BattleCondition_Conditions.totalActionsUsed =>
                Calculate(target != null ? target.TotalActionsUsed : 0),

            Enum_BattleCondition_Conditions.isAllied =>
                Calculate(checker && checker.team.CompareAllied(target.team) ? 1 : 0),

            Enum_BattleCondition_Conditions.isOpponent =>
                Calculate(checker && checker.team.CompareEnemy(target.team) ? 1 : 0),

            Enum_BattleCondition_Conditions.isSelf =>
                Calculate(checker && checker == target ? 1 : 0),

            Enum_BattleCondition_Conditions.isAlive =>
                Calculate(target.hero != null && target.hero.health.IsAlive ? 1 : 0),

            Enum_BattleCondition_Conditions.isDefeated =>
                Calculate(target.hero != null && target.hero.health.IsDead ? 1 : 0),

            Enum_BattleCondition_Conditions.hasStatusConditionID =>
                Calculate(target.HasStatus((int)value) ? 1f : 0f),

            Enum_BattleCondition_Conditions.survivingAllies =>
                Calculate(CountSurvivors(checker, allied: true, excludeSelf: true)),

            Enum_BattleCondition_Conditions.survivingEnemies =>
                Calculate(CountSurvivors(checker, allied: false, excludeSelf: true)),

            Enum_BattleCondition_Conditions.moveIsPhysical =>
                Calculate(move != null && move.damageType == DamageTypes.physical ? 1 : 0),

            Enum_BattleCondition_Conditions.moveIsMagical =>
                Calculate(move != null && move.damageType == DamageTypes.magical ? 1 : 0),

            Enum_BattleCondition_Conditions.moveContainsDamage =>
                Calculate(move != null && move.HasEffect(Enum_BattleEffect_Effects.damage) ? 1 : 0),

            Enum_BattleCondition_Conditions.moveIsMelee =>
                Calculate(move != null && move.melee ? 1 : 0),

            Enum_BattleCondition_Conditions.moveContainsDebuff =>
                Calculate(move != null && move.HasEffect(Enum_BattleEffect_Effects.modifyAttributes) ? 1 : 0),

            Enum_BattleCondition_Conditions.moveContainsStatusCondition =>
                Calculate(move != null && move.HasEffect(Enum_BattleEffect_Effects.applyStatusCondition) ? 1 : 0),

            Enum_BattleCondition_Conditions.moveHitsAoE =>
                Calculate(move != null && move.range != RangeExtension.none ? 1 : 0),

            Enum_BattleCondition_Conditions.moveIsAction =>
                Calculate(move != null && move.moveType == MoveType.action ? 1 : 0),

            Enum_BattleCondition_Conditions.moveIsReaction =>
                Calculate(move != null && move.moveType == MoveType.reaction ? 1 : 0),

            Enum_BattleCondition_Conditions.moveIsSpecial =>
                Calculate(move != null && move.moveType == MoveType.special ? 1 : 0),

            Enum_BattleCondition_Conditions.moveIsActionOrSpecial =>
                Calculate(move != null && (move.moveType == MoveType.action || move.moveType == MoveType.special) ? 1 : 0),

            Enum_BattleCondition_Conditions.tileType =>
                Calculate(HexWalker.DisputedTerritory ? (int)HexWalker.DisputedTerritory.type : -1),

            Enum_BattleCondition_Conditions.isLeader =>
                Calculate(target.parent.parent.Leader() == target.hero ? 1 : 0),

            _ => false,
        };
    }


    protected bool Calculate(float input)
    {
        return comparison switch
        {
            Enum_BattleCondition_Comparison.greaterThan => input > value,
            Enum_BattleCondition_Comparison.greaterThenOrEqualTo => input >= value,
            Enum_BattleCondition_Comparison.lesserThan => input < value,
            Enum_BattleCondition_Comparison.lesserThanOrEqualTo => input <= value,
            Enum_BattleCondition_Comparison.notEqual => input != value,
            //Equal To
            _ => input == value,
        };
    }
    // Add these helpers inside BattleCondition class:
    private static int CountSurvivors(BattleCombatant checker, bool allied, bool excludeSelf = true)
    {
        if (checker == null) return 0;

        BattleCombatant[] all = allied ? checker.GetAllAllies() : checker.GetAllEnemies();
        if (all == null || all.Length == 0) return 0;

        int count = 0;
        for (int i = 0; i < all.Length; i++)
        {
            BattleCombatant next = all[i];
            if (excludeSelf && next == checker) continue;
            else if(next.IsAlive) count++;
        }
        return count;
    }
}
