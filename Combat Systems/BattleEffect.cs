using NUnit.Framework.Internal;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Enum_BattleEffect_Effects
{
    damage = 1, loseHealth = 2,
    physicalDamage = 3, magicalDamage = 4,
    actions = 5,

    mana = 6, // new

    shield = 10, 
    heal = 20,
    modifyAttributes = 30,
    applyStatusCondition = 40,
    shatter = 60,
    redirect = 80,
    counterspell = 100,

    copy = 200,
}
[System.Serializable]
public class BattleEffect
{
    public static BattleEvent lastAction;

    public enum EffectTarget
    { other, self }
    public EffectTarget target;
    public Enum_BattleEffect_Effects effect;
    [Tooltip("[Health Values]\n" +
        "0 : Regular\n1 : Percentage Max\n2 : Percentage Current\n3 : Percentage Missing\n4 : Flat Damage\n" +
        "\n[Attribute Indexes]" +
        "\nhealth = 0,\nmana = 1, \npower = 2, \narmor = 3, " +
        "\nmagic = 4, \nresist = 5, \nspeed = 6, \naccuracy = 7, " +
        "\nevasion = 8, \nshield = 9,\nactions = 10,\nreactions = 11," +
        "\nactions = 12" +
        "\nreactions = 13; " +
        "\ncriticalChance = 14; " +
        "\ncriticalDamage = 15; " +
        "\npotential = 0; \n\n"
        + "- ACTIONS -\n" +
        "0: Action Points\n1: Reaction Points\n\n" +
        "[DAMAGE] \nIndex Positive : Life Steal\nIndex Negative: Recoil Damage")]
    public int index = 0;
    public float weight = 50f;
    public BattleCondition[] conditions;

    public void Parse(BattleEvent action, float mod = 1f)
    {  
        lastAction = action;
        foreach(var target in action.targets)
        {
            Apply(action.source, target, mod); 
        }
    }
    public void Apply(BattleCombatant source, BattleCombatant target, float mod = 1f)
    {
        foreach(var item in conditions)
        {
            if (item.RunCondition(source, lastAction) == false)
                return;
        }

        if (this.target == EffectTarget.self)
            target = source;

        switch(effect)
        {
            default: Debug.Log("<color=red>BATTLE EFFECT</color> : No implementation for effect value " + effect);
                break;// Nothing ?
            case Enum_BattleEffect_Effects.damage:
                DealDamage(source, target, mod);
                break;
            case Enum_BattleEffect_Effects.physicalDamage:
                DealDamage(source, target, mod);
                break;
            case Enum_BattleEffect_Effects.magicalDamage:
                DealDamage(source, target, mod);
                break;
            case Enum_BattleEffect_Effects.loseHealth:
                ApplyHealthLoss(source, target, mod);
                break;
            case Enum_BattleEffect_Effects.mana:
                ApplyManaChange(source, target, mod);
                break;
            case Enum_BattleEffect_Effects.actions:
                ModifyActions(source, target);
                break; 
            case Enum_BattleEffect_Effects.heal:
                RestoreHealth(source, target, mod);
                break;
            case Enum_BattleEffect_Effects.shield:
                AddShielding(source, target, mod);
                break;
            case Enum_BattleEffect_Effects.shatter:
                ShatterShield(source, target, mod);
                break; 
            case Enum_BattleEffect_Effects.modifyAttributes:
                ModifyAttributes(source, target);
                break; 
            case Enum_BattleEffect_Effects.applyStatusCondition:
                ApplyStatusCondition(source, target, mod);
                break;

            case Enum_BattleEffect_Effects.redirect:
                ChangeTargetOnStackTo(source, target);
                break;

            case Enum_BattleEffect_Effects.counterspell:
                RemoveSkillOnStack(target);
                break;

            case Enum_BattleEffect_Effects.copy:
                CopyAndAddMinion(target);
                break;
        }
    }

    private void CopyAndAddMinion(BattleCombatant target)
    {
        BattleUnit parent = target.parent;
        parent.CreateCopy(target, weight);
    }

    private void ModifyActions(BattleCombatant source, BattleCombatant target)
    {
        switch(index)
        {
            default: //AP
                target.actionPoints += Mathf.RoundToInt(weight);
                target.PrintPopUp($"{Mathf.RoundToInt(weight)} AP", Color.indianRed);
                break;

            case 1: //RP
                target.reactionPoints += Mathf.RoundToInt(weight);
                target.PrintPopUp($"{Mathf.RoundToInt(weight)} RP", Color.cadetBlue);
                break;

        }
    }

    private void RemoveSkillOnStack(BattleCombatant target)
    {
        BattleEvent hit = null;
        foreach(var skill in AutoBattleManager.Stack)
        {
            if (skill.source == target)
                hit = skill;
        }
        if (hit == null) return; 

        AutoBattleManager.RemoveFromStack(hit);

        if (index == 1) hit.source.SpendAP_RP(hit.move);

        target.PrintPopUp("COUNTERED", Color.cornflowerBlue);
    }

    private void ChangeTargetOnStackTo(BattleCombatant source, BattleCombatant target)
    {
        switch(index)
        {
            default: // Cover: redirect attacks from "protected" (ally) to "source" (the cover user)

                foreach (var skill in AutoBattleManager.Stack)
                { 
                    if (!skill.targets.Contains(target)) continue;
                    if (skill.targets.Contains(source) && source.team != target.team) 
                        skill.targets.Remove(target);

                    else skill.targets[skill.targets.IndexOf(target)] = source;
                }
                break;

            case 1: // TAUNT : Change the targets in a skill owned by the target to be only the user
                foreach (var skill in AutoBattleManager.Stack)
                    if (skill.owner == target)
                        skill.targets = new() { source };
                break;
        }
    }

    public void ApplyStatusCondition(BattleCombatant source, BattleCombatant target, float mod)
    {
        Passive effect = Passive.Get(index);
        StatusCondition status = new()
        {
            status = effect,
            source = source,
            target = target,
            stacks = mod,
            duration = effect.duration
        };
        target.GainStatus(source, status);
    }

    public void DealDamage(BattleCombatant source, BattleCombatant target, float mod = 1f)
    {
        float damage = RollDamage(source, target);

        if (damage <= 0f) return;

        Debug.Log($"{source.FullTeamName} is dealing <color=red><b>{damage}</b></color> damage to {target.FullTeamName}");

        target.TakeDamage(new(lastAction.move, lastAction.owner, source, target, damage * mod));
        if(index != 0) // life drain / life steal
        { // If positive, recovery

            float percent = (float)index / 100f;
            float recoil = damage * percent;
            if (recoil < 0)
                source.TakeDamage(new(lastAction.move, lastAction.owner, source, source, recoil));

            else source.GainHealth(new(lastAction.move, lastAction.owner, source, source, -recoil));
        }
    }
    public float RollDamage(BattleCombatant source, BattleCombatant target, float mod = 1f)
    { 
        float variance = Random.Range(0.9f, 1.1f);

        float offense = GetOffense(source);
        float defense = GetDefense(target);  
        Equalize(ref offense, ref defense);

        float contest = offense / Mathf.Max(1f, defense);
        float basePower = Mathf.Sqrt(weight * offense) * contest; 

        return CheckFatigue(Mathf.Ceil(basePower * variance * mod), source, target);
    }
    public float CheckFatigue(float input, BattleCombatant source, BattleCombatant target)
    { 
        input = FatigueModifierOffense(input, source.parent.GetMapParent());
        input = FatigueModifierDefense(input, target.parent.GetMapParent());

        return input;
    }
    public float FatigueModifierOffense(float input, HexWalker walker)
    {
        if (walker == null) return input;

        float fatigue = 1f + (walker.fatigue / 20);
        return input / fatigue;
    }
    public float FatigueModifierDefense(float input, HexWalker walker)
    {
        if (walker == null) return input;

        float fatigue = 1f + (walker.fatigue / 20);
        return input * fatigue;
    }

    public void ApplyHealthLoss(BattleCombatant source, BattleCombatant target, float mod = 1f)
    { 
        target.LoseHealth(new()
        {
            eventType = Enum_BattleEvent_Type.onHealthLost,
            owner = target,
            source = source,
            targets = new() { target },
            value = Mathf.CeilToInt(LoseHealth(source, target) * mod),
        });
    }
    public void ApplyManaChange(BattleCombatant source, BattleCombatant target, float mod = 1f)
    {
        if(weight > 0) // gain mana
        {
            target.GainMana(new()
            {
                owner = target,
                source = source,
                targets = new() { target },
                value = Mathf.Abs(weight * mod),
                eventType = Enum_BattleEvent_Type.onManaGained,
                
            });
        }
        else // lose mana
        {
            target.DamageMana(new()
            {
                owner = target,
                source = source,
                targets = new() { target },
                value = Mathf.Abs(weight * mod),
                eventType = Enum_BattleEvent_Type.onManaGained, 
            });
        }
    }
    public float LoseHealth(BattleCombatant source, BattleCombatant target)
    {
        switch (index)
        {
            // 0: Roll contest
            default:
            case 0: return RollDamage(source, target); 

            // 1: % of MAX health
            case 1:
                {
                    float maxHP = target.hero.health.health_max;
                    return Mathf.Ceil(maxHP * weight);
                }

            // 2: % of CURRENT health
            case 2:
                {
                    float currentHP = target.hero.health.health_current;
                    return Mathf.Ceil(currentHP * weight);
                }

            // 3: % of MISSING health
            case 3:
                {
                    float missingHP = target.hero.health.health_max - target.hero.health.health_current;
                    if (missingHP <= 0f) return 0f;
                    return Mathf.Ceil(missingHP * weight);
                }

            // 4: Flat damage
            case 4:
                {
                    return Mathf.Ceil(weight);
                }
        }
    }

    public float GetOffense(BattleCombatant source)
    {
        if (effect == Enum_BattleEffect_Effects.physicalDamage)
            return source.Attributes.power;
        if (effect == Enum_BattleEffect_Effects.magicalDamage)
            return source.Attributes.magic;

        if (lastAction.move == null) return 1f;

        if (lastAction.move.damageType == DamageTypes.physical)
            return source.Attributes.power;
        else if (lastAction.move.damageType == DamageTypes.magical)
            return source.Attributes.magic;

        else return 1f;
    }
    public float GetDefense(BattleCombatant source)
    {
        if (effect == Enum_BattleEffect_Effects.physicalDamage)
            return source.Attributes.armor;
        if (effect == Enum_BattleEffect_Effects.magicalDamage)
            return source.Attributes.resist;

        if (lastAction.move == null) return source.Attributes.armor;

        if (lastAction.move.damageType == DamageTypes.physical)
            return source.Attributes.armor;
        else if (lastAction.move.damageType == DamageTypes.magical)
            return source.Attributes.resist;

        else return 1f;
    }
    public static void Equalize(ref float A, ref float B)
    {
        float diff = Mathf.Abs(A - B);
        if (A < B) A += diff / 2;
        else if(B < A) B += diff / 2;
    }
    public void RestoreHealth(BattleCombatant source, BattleCombatant target, float mod = 1f)
    { 
        float heal = RollHealing(source, target);
        if (heal <= 0f) return;

        Debug.Log($"{source.FullTeamName} healed <color=green><b>{heal}</b></color> health to {target.FullTeamName}");

        target.GainHealth(new()
        { 
            source = source,
            targets = new() { target },
            value = heal,
        });
    }

    public float RollHealing(BattleCombatant source, BattleCombatant target, bool randomize = true)
    {
        float variance = randomize ? Random.Range(0.9f, 1.1f) : 1f;

        switch (index)
        {
            // 0: Standard healing (existing behavior)
            default:
            case 0:
                {
                    float power = Mathf.Sqrt(GetOffense(source) * weight * 2);
                    return Mathf.Ceil(power * variance);
                }

            // 1: % of MAX health
            case 1:
                {
                    float maxHP = target.hero.health.health_max;
                    return Mathf.Ceil(maxHP * weight * variance);
                }

            // 2: % of CURRENT health
            case 2:
                {
                    float currentHP = target.hero.health.health_current;
                    return Mathf.Ceil(currentHP * weight * variance);
                }

            // 3: % of MISSING health
            case 3:
                {
                    float missingHP = target.hero.health.health_max - target.hero.health.health_current;
                    if (missingHP <= 0f) return 0f;
                    return Mathf.Ceil(missingHP * weight * variance);
                }

            // 4: Flat healing
            case 4:
                {
                    return Mathf.Ceil(weight * variance);
                }
        }
    }

    public void AddShielding(BattleCombatant source, BattleCombatant target, float mod = 1f, bool randomize = true)
    {

        if (index == 1)
            target.GainShield(new()
            {
                eventType = Enum_BattleEvent_Type.onShieldGained,
                owner = target,
                source = source,
                targets = new() { target },
                value = weight * mod,
            });

        else ShieldByScaling(source, target, mod, randomize); 
    }
    private void ShieldByScaling(BattleCombatant source, BattleCombatant target, float mod = 1f, bool randomize = true)
    { 
        float defense = GetDefense(source);
        float varience = randomize ? Random.Range(0.9f, 1.1f) : 1f;
        float amount = Mathf.CeilToInt(Mathf.Sqrt(weight * defense) * varience * mod); 

        Debug.Log($"{source.FullTeamName} is shielding <color=cyan><b>{amount}</b></color> to {target.FullTeamName}");
        target.GainShield(new()
        {
            eventType = Enum_BattleEvent_Type.onShieldGained,
            owner = target,
            source = source,
            targets = new() { target },
            value = amount,
        });
    }
    public void ShatterShield(BattleCombatant source, BattleCombatant target, float mod = 1f)
    {
        if (target.hero.health.shield_current <= 0) return;

        float damage = Mathf.CeilToInt(RollDamage(source, target));

        Debug.Log($"{source.FullTeamName} is melting <color=orange><b>{damage}</b></color> shield to {target.FullTeamName}");

        target.ShatterShield(new(lastAction.move, lastAction.owner, source, target, damage * mod));
    } 
    public void ModifyAttributes(BattleCombatant source, BattleCombatant target)
    {
        if (target == null || target.hero == null) return;

        Enum_HeroAttributes stat = (Enum_HeroAttributes)Mathf.Clamp(index, 0, 11); 
           
        target.modifiers.MultiplyStat(stat, weight);

        target.Emit(new()
        {
            eventType = weight > 1 ? Enum_BattleEvent_Type.onAttributeBuffed : Enum_BattleEvent_Type.onAttributeDebuff,
            owner = target,
            source = source,
            targets = new() { target },
            value = weight,
        });
        Debug.Log($"{source} Modified {target} {stat} to {weight}");


        float printValue = weight - 1f;
        UI_PopUpTrigger.CreatePopUp($"{printValue:0%} {Attributes.PrintAttributeName(stat)}", Color.white, target.transform.position, 64);
    }

}
