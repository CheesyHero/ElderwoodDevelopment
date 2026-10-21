using UnityEngine;

public class Skill : ScriptableObject
{
    public string skillName = "new skill";
    [TextArea]
    public string description = "a new move that does things";

    public virtual string PrintTooltip()
    {
        return $"{skillName} | {description}";
    }
}
