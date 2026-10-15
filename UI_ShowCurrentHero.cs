using UnityEngine;

public class UI_ShowCurrentHero : MonoBehaviour
{
    public bool showPromotion = false;
    public Hero lastInput;
    [SerializeField] ComponentLoader[] children;
    private void Awake()
    {
        children = GetComponentsInChildren<ComponentLoader>();
        UI_FullArmyDisplay.OnUpdateHero += LoadHero;
        UI_UnitSlot.OnHoverPreviewEnter += PreviewHero;
        UI_UnitSlot.OnHoverPreviewExit += ReturnToLast;
    }
    private void OnDestroy()
    {
        UI_FullArmyDisplay.OnUpdateHero -= LoadHero;
        UI_UnitSlot.OnHoverPreviewEnter -= PreviewHero;
        UI_UnitSlot.OnHoverPreviewExit -= ReturnToLast;
    }

    public void PreviewHero(Hero hero)
    {
        //Debug.Log($"<color=cyan>ENTER</color> | {hero}");
        Pass(hero);
    }

    public void ReturnToLast()
    {
        //Debug.Log($"<color=red>EXIT</color> | {lastInput} ({UI_FullArmyDisplay.CurrentHero})");
        Pass(lastInput);
    }

    public void LoadHero(Hero hero)
    {  
        lastInput = hero;
        Pass(lastInput);
    }
    public void Pass(Hero hero)
    { 
        if (showPromotion && hero != null) 
            hero = hero.heroClass.promotion ? hero.Copy(hero.heroClass.promotion) : null;

        foreach (var loader in children)
            loader.PassObject(hero);
    }
}
