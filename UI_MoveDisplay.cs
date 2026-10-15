using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_MoveDisplay : ComponentLoader, IPointerEnterHandler, IPointerExitHandler
{ 
    public Skill last;
    public Button button;
    public Color reactionColor, actionColor, specialColor, passiveColor, emptyColor;
    public Color leaderActiveColor, followerColor;
    public Vector2 tooltipOffset = new();
    public override void PassObject(object obj)
    { 
        last = null;
        if (obj is BattleMove move)
        {
            button.enabled = true;
            SetText(move.skillName);
            SetImageColor(GetMoveTypeColor(move));

            last = move;
        }
        else if (obj is Passive passive)
        {
            button.enabled = false;
            SetText(passive.skillName);
            SetImageColor(passiveColor);

            last = passive;
        }
        else if (obj is Hero hero)
        {
            button.enabled = false;
            Passive p = hero.heroClass.leaderTraits.passive;

            if (p == null)
                EmptyPrint();

            else
            {
                SetText(p.skillName);
                SetImageColor(hero == UI_FullArmyDisplay.CurrentUnit?.Leader() ?
                    leaderActiveColor : followerColor);

                last = p;
            } 
        }
        else EmptyPrint();
    }
    protected void EmptyPrint()
    {
        button.enabled = true;
        SetText("No Move");
        SetImageColor(emptyColor);
    }
    protected Color GetMoveTypeColor(BattleMove move)
    {
        return move.moveType switch
        {
            MoveType.reaction => reactionColor,
            MoveType.special => specialColor,
            _ => actionColor,
        };
    }
    public void ChangeMove()
    {
        if (!last) return;

        UI_FullArmyDisplay.ChangeMove(last as BattleMove, UI_SkillSlotOption.desiredIndex);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (last != null)
            TooltipManager.OpenTooltip(last.PrintTooltip(), (Vector2)transform.position + tooltipOffset);
    }

    public void OnPointerExit(PointerEventData eventData) 
        => TooltipManager.CloseTooltip();
}
