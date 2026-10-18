// Equipment.cs
using System;
using System.Text;
using UnityEngine;

public enum Enum_Equipment_Types
{
    sword, axe, hammer, pick, spear, bow,
    robes, lightArmor, heavyArmor, shields,
    hat, helmet, boots, gauntlets, accessory,
    instrument, staff, wand, spellbook,
}

[CreateAssetMenu(fileName = "New Equipment", menuName = "NMD/RPG/Equipment", order = 3)]
public class Equipment : Item
{
    [Header("Equipment Attributes")]
    public Enum_Equipment_Types equipmentType;
    public Attributes stats;
    public BattleMove[] skills;
    public Passive[] effects;

    public override string PrintToolTip(bool title)
    {
        StringBuilder sb = new();
        sb.Append($"{(title ? $"{itemName} : " : "")}<u>{rarity} {equipmentType}</u>\n\n");

        if (stats != null)
        {
            foreach (Enum_HeroAttributes a in Enum.GetValues(typeof(Enum_HeroAttributes)))
            {
                float v = stats.GetStat(a);
                if (Mathf.Approximately(v, 0f))
                    continue;

                bool forceInt = (a == Enum_HeroAttributes.actions || a == Enum_HeroAttributes.reactions);
                string num = forceInt
                    ? $"{(v > 0 ? "+" : "")}{Mathf.RoundToInt(v)}"
                    : $"{(v > 0 ? "+" : "")}{v:0.##}";

                string color = v > 0 ? "green" : "red";
                sb.Append($" ~ {PrettyStatName(a)} <color={color}>{num}</color>\n");
            }
        }

        if (skills != null && skills.Length > 0)
        {
            sb.Append("\n");

            foreach (var item in skills)
            {
                if (item == null) continue;
                sb.Append(item.PrintTooltip());
            }
        }

        if (effects != null && effects.Length > 0)
        {
            sb.Append("\n");

            foreach (var passive in effects)
            {
                if (passive == null) continue;
                sb.Append(passive.PrintTooltip());
            }
        }

        return sb.ToString();
    }
    public override string PrintShortTooltip()
    {
        StringBuilder sb = new();

        // Attributes
        if (stats != null)
        {
            foreach (Enum_HeroAttributes a in Enum.GetValues(typeof(Enum_HeroAttributes)))
            {
                float v = stats.GetStat(a);
                if (Mathf.Approximately(v, 0f))
                    continue;

                bool forceInt = (a == Enum_HeroAttributes.actions ||
                                 a == Enum_HeroAttributes.reactions);

                string num = forceInt
                    ? $"{(v > 0 ? "+" : "")}{Mathf.RoundToInt(v)}"
                    : $"{(v > 0 ? "+" : "")}{v:0.##}";

                string color = v > 0 ? "green" : "red";

                sb.AppendLine($" ~ {PrettyStatName(a)} <color={color}>{num}</color>");
            }
        }

        // Abilities
        if (skills != null)
        {
            foreach (var skill in skills)
            {
                if (skill == null) continue;
                sb.AppendLine($" ~ {skill.skillName}");
            }
        }

        // Passives
        if (effects != null)
        {
            foreach (var passive in effects)
            {
                if (passive == null) continue;
                sb.AppendLine($" ~ {passive.skillName}");
            }
        }

        return sb.ToString().TrimEnd();
    }

    private static string PrettyStatName(Enum_HeroAttributes a)
    {
        return Attributes.PrintAttributeName(a);
    }
    public override string ToString()
    {
        string hex = ColorUtility.ToHtmlStringRGB(color);
        return $"<color=#{hex}>{itemName}</color> - {BattleConstants.WrapWithBadgeColor(rarity.ToString(), rarity)} " +
            $"{RandomNameGenerator.Capitalize(equipmentType.ToString())}</color>";
    }
}