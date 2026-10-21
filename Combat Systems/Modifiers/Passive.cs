using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

[CreateAssetMenu(
    fileName = "New Skill",
    menuName = "NMD/RPG/Passive Effects"
)]
public class Passive : Skill
{   
    public Color color; 
    public int duration = -1;
    public bool compound = true;
    public ConditionEvent[] effects;
    public Modifier[] modifiers;

    readonly static List<Passive> all = new();
    public static Passive[] AllEffects
    {
        get
        {
            if(all.Count <= 0)
                all.AddRange(Resources.LoadAll<Passive>("Statuses"));

            return all.ToArray();
        }
    }
    public static Passive Get(int index)
    {
        foreach (var item in AllEffects)
        {
            if (MatchesIndex(index, item.name))
                return item;
        }

        return null;
    }
    public static bool MatchesIndex(int index, string value)
    {
        string token = index.ToString("D3"); // 8 -> "008"
        return value != null && value.Contains(token);
    }
    public int GetID()
    {
        int start = name.IndexOf("Status", System.StringComparison.Ordinal);
        string digits = name.Substring(start + 6, 3); // after "Status"
        return int.Parse(digits);
    }

    public int ApplyEffects(BattleCombatant afflicted, BattleEvent action, float stacks)
    {
        int totalCost = 0;
        foreach(var effect in effects)
            totalCost += effect.Parse(afflicted, action, stacks);

        return totalCost;
    } 
    public void ApplyModifiers(Enum_BattleEvent_Modifier effect, BattleCombatant afflicted, BattleEvent action, float input)
    {
        foreach (var mod in modifiers)
            mod.Parse(effect, afflicted, action, input);
    }
    public override string PrintTooltip()
    {
        StringBuilder sb = new();

        sb.Append($"<color=#e3d754><b>Passive</color>: <u><color=#{ColorUtility.ToHtmlStringRGB(color)}>{skillName}</u></b></color>\n");

        if (!string.IsNullOrWhiteSpace(description))
            sb.Append($"{description}"); 

        return sb.Append("\n").ToString();
    }
}

[System.Serializable]
public class ConditionEvent
{
    public int maxTriggers = -1;
    public int durationCost = 0;
    public string eventPrint = "Pop!";
    public Color textColor = Color.white; 
    public float perStackMult = 0.0f;
    public TriggerCondition[] triggerEvents; 
    public BattleEffect[] effects;

    public int Parse(BattleCombatant afflicted, BattleEvent action, float stacks)
    {
        if (stacks <= 0) return 0;
        if (afflicted == null || action == null) return 0;

        bool anyTriggered = false;

        BattleEvent afflictedView = action.Copy(afflicted, action.eventType); 

        foreach (var trigger in triggerEvents)
        {
            if (trigger.TryResolveTargets(afflicted, afflictedView, out var results))
            {
                anyTriggered = true;

                if (eventPrint.Length > 0)
                    afflicted.PrintPopUp(eventPrint, textColor);

                foreach (var effect in effects)
                {
                    effect.Parse(new BattleEvent()
                    {
                        move = afflictedView.move,
                        eventType = Enum_BattleEvent_Type.onStatusEffect,
                        owner = afflicted,
                        source = afflictedView.source,
                        targets = results,
                        value = -1,
                    }, (1f - perStackMult) + (stacks * perStackMult));
                }
            }
        }

        return anyTriggered ? durationCost : 0;
    }


}
