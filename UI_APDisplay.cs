using UnityEngine;

public class UI_APDisplay : MonoBehaviour
{
    public enum Modes { actionPoints, reactionPoints }
    public Modes mode;
    public BattleCombatant parent;
    public GameObject[] children; 
    private void Update()
    {
        if (!parent || children.Length <= 0) return;

        for(int i = 0; i < children.Length; i++)
        {
            var child = children[i];
            child.SetActive(GetFromMode() > i);
        }
    }
    private int GetFromMode()
    {
        if (!parent || parent.hero.health.IsDead) return 0;
        else return mode == Modes.actionPoints ? parent.actionPoints : parent.reactionPoints;
    }
}
