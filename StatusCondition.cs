using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class StatusCondition 
{
    private readonly List<long> completedIDs = new();
    public Passive status;
    public BattleCombatant target, source;
    public float stacks = 0f;
    public int duration = -1; 

    public bool Positive { get { return status ? status.positive : false; } }

    public void Apply(BattleCombatant hero, BattleEvent action)
    {
        if (hero == null || completedIDs.Contains(action.instanceId)) return;

        int cost = status.ApplyEffects(hero, action, stacks);
        if(duration >= 0) duration = Mathf.Max(duration - cost, 0); 

        completedIDs.Add(action.instanceId);
    } 
    public static StatusCondition operator +(StatusCondition a, StatusCondition b)
    {
        if(a.status == null || b.status == null) 
            return a;
        if(a.status != b.status)
            return a;

        if (a.status.compound)
            return new StatusCondition()
            {
                source = a.source,
                status = a.status,
                target = a.target,
                stacks = a.stacks + b.stacks,
            };

        else // take highest
            return new StatusCondition()
            {
                source = a.source,
                status = a.status,
                target = a.target,
                stacks = a.stacks > b.stacks ? a.stacks : b.stacks,
            };
    } 
} 