using UnityEngine;
using UnityEngine.UI;

public class UI_PromotionButton : ComponentLoader
{
    public Button button;
    public CanvasGroup group;  

    private void Start()
    {
        UI_FullArmyDisplay.OnUpdateHero += PassObject;
        UI_UnitSlot.OnHoverPreviewEnter += PassObject;
        UI_UnitSlot.OnHoverPreviewExit += ReturnToLast;
    }

    private void OnDestroy()
    {
        UI_FullArmyDisplay.OnUpdateHero -= PassObject;
        UI_UnitSlot.OnHoverPreviewEnter -= PassObject;
        UI_UnitSlot.OnHoverPreviewExit -= ReturnToLast;
    }
       
    public void ReturnToLast()
    { 
        PassObject(UI_FullArmyDisplay.CurrentHero);
    }
     
    public override void PassObject(object obj)
    {
        if (obj is Hero hero && hero != null && hero.heroClass != null && hero.heroClass.promotion)
        {
            SetText($"Promote {hero} to {hero.heroClass.promotion.className}?");
            bool RequirementMet = hero.heroClass.HasPromotionRequirement();
            if (button) button.interactable = RequirementMet;
            if(group) group.alpha = RequirementMet ? 1 : 0.33f;
        }
        else
        {
            SetText("No promotion available");
            if (button) button.interactable = false;
            if (group) group.alpha = 0.5f;
        }
    }
}
