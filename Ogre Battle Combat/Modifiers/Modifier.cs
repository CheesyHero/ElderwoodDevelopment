using UnityEngine;

public enum Enum_BattleEvent_Modifier
{
    damageOut, damageIn,
    hitChance, dodgeChance,
    healingOut, healingIn,
    manaOut, manaIn,
    armorOut, armorIn, 
    initiative, 
    actionCost, reactionCost,
    criticalChance, criticalDamage,
    criticalEvade, criticalResist,
}

[System.Serializable]
public class Modifier
{
    public int priority = 0;
    public enum Calculation { add, subtract, multiply, divide }
    public Calculation calculation = Calculation.multiply;
    public Enum_BattleEvent_Modifier modifier;
    public float weight = 1f; 
    public BattleCondition[] conditions;

    public float Parse(Enum_BattleEvent_Modifier effect, BattleCombatant afflicted, BattleEvent action, float input)
    {
        // Debug.LogWarning($"<color=orange>Testing {effect} on{afflicted}");

        if (conditions.Length > 0)
            foreach (var item in conditions)
                if (item.RunCondition(afflicted, action) == false)
                    return input;

        if (effect == modifier)
        {
            // Debug.LogWarning($"<color=white>effect of {effect} has been found for {afflicted}");
            return calculation switch
            {
                Calculation.add => input + weight,
                Calculation.subtract => input - weight,
                Calculation.divide => input / weight,
                _ => input * weight,
            };
        } 

        return input;
    }
}
