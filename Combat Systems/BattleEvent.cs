using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Enum_BattleEvent_Type
{
    onDamageDealt, onDamageTaken, onDamageBlocked,
    onHealthGained, onHealthLost,
    onShieldDamaged, onShieldGained, onShieldBroken,
    onManaSpent, onManaLoss, onManaGained,
    onMoveUsed, onBeingTargeted, onAfterTargeted,
    onBattleStart, onBattleEnd,
    onTurnStart, onTurnEnd,
    onDefeated, onRevived,
    onStatusApplied, onStatusRemoved, onStatusEffect, 
    onMoveEvaded, onMoveMissed,
    onRoundEnd, onRoundStart,
    onAttributeBuffed, onAttributeDebuff,
    onReactionUsed, onHit

}
public class BattleEvent
{
    public long instanceId = 0;
    public Enum_BattleEvent_Type eventType;
    public BattleMove move;
    public BattleCombatant owner; // The source of the trigger
    public BattleCombatant source; // Who caused the event. EX: Enemy who attacked an ally
    public List<BattleCombatant> targets = new(); // Who was effected by the original passive or attack
    public float value; // IE Damage or healing
    static BattleMove defaultMove;
    public BattleMove DesiredMove
    {
        get
        {
            if (move == null)
            {
                if (defaultMove == null)
                    defaultMove = Resources.Load<BattleMove>("Moves/Actions/Move000_Standby");
                return defaultMove;
            }
            else return move;
        }
    }

    public BattleEvent()
    { }
    public BattleEvent(BattleMove m, BattleCombatant A, BattleCombatant B, BattleCombatant C, float value)
    {
        move = m;
        owner = A; 
        source = B;
        targets = new() { C };
        this.value = value;
    } 
    public BattleEvent(BattleEvent copy, Enum_BattleEvent_Type newType)
    {
        instanceId = copy.instanceId;
        move = copy.move;
        owner = copy.owner; 
        source = copy.source; 
        targets = copy.targets;
        value = copy.value;
        eventType = newType;
    }
    public BattleEvent(BattleEvent copy, BattleCombatant newOwner, Enum_BattleEvent_Type newType)
    {
        instanceId = copy.instanceId;
        move = copy.move;
        owner = newOwner; 
        source = copy.source; 
        targets = copy.targets;
        value = copy.value;
        eventType = newType;
    }
    public BattleEvent Copy(Enum_BattleEvent_Type newType)
    {
        return new(this, newType);
    }
    public BattleEvent Copy(BattleCombatant newOwner, Enum_BattleEvent_Type newType)
    {
        return new(this, newOwner, newType);
    }
    public void Push()
    {
        Debug.Log($"Pushing next action = {move}");
        owner.UseMove(this);
    } 
} 
