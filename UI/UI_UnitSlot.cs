using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;

public class UI_UnitSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public int x, y;
    public static Vector2 LastIndex;
    [Space(4)]
    protected Unit parent = null;
    protected Hero lastInput = null;
    [SerializeField]
    protected Image heroSprite;
    [SerializeField]
    protected Image leaderDisplay; 

    public Button[] subButtons;

    [SerializeField] private Material teamColorMaterial;
    private Material heroMaterial;

    public static UnityAction<Hero> OnHoverPreviewEnter;
    public static UnityAction OnHoverPreviewExit;

    protected ComponentLoader[] children;

    protected virtual void Awake()
    {
        children = GetComponentsInChildren<ComponentLoader>();

        UI_FullArmyDisplay.OnUpdateHero += Reload;
        UI_FullArmyDisplay.OnUpdateUnit += Reload;

        if (heroSprite && teamColorMaterial)
        {
            heroMaterial = new Material(teamColorMaterial);
            heroSprite.material = heroMaterial;
        } 
    }
    private void OnDestroy()
    {
        OnHoverPreviewEnter = null;
        OnHoverPreviewExit = null;

        UI_FullArmyDisplay.OnUpdateHero -= Reload;
        UI_FullArmyDisplay.OnUpdateUnit -= Reload;

        if (heroMaterial)
            Destroy(heroMaterial);
    }

    public void Reload()
    {
        if (parent != null) LoadUnit(parent);
    }
    public void Reload(Hero _) => Reload();
    public void Reload(Unit _) => Reload();

    public virtual void LoadUnit(Unit unit)
    {
        parent = unit;
        Hero hero = parent.GetFromPlacement(x, y);
        bool leader = unit.Leader() == hero;
        LoadHero(hero, leader);  
    }
    public virtual void LoadHero(Hero hero, bool leader)
    {
        //Debug.Log($"{name} : Attempting to load hero : {hero} // as leader = {leader}");
        if (!heroSprite || children == null) return;

        heroSprite.enabled = hero != null; 
        heroSprite.sprite = hero?.heroClass.actorImage; if (heroMaterial)
        {
            Texture2D mask = hero?.heroClass?.colorMask != null
                ? hero.heroClass.colorMask
                : Texture2D.blackTexture;

            Color teamColor = hero?.team != null
                ? hero.team.teamColor
                : Color.white;

            Color armorColor = hero?.equipment?.armor != null
                ? hero.equipment.armor.color
                : Color.white;

            Color weaponColor = hero?.equipment?.weapon != null
                ? hero.equipment.weapon.color
                : Color.white;

            heroMaterial.SetTexture("_MaskTex", mask);
            heroMaterial.SetColor("_TeamColor", teamColor);
            heroMaterial.SetColor("_ArmorColor", armorColor);
            heroMaterial.SetColor("_WeaponColor", weaponColor);
        }

        lastInput = hero;

        foreach (var item in children)
            item.PassObject(hero);
        // Debug.Log($"{name} : Should have hero = {(hero != null ? hero : "null")}");

        if (leaderDisplay)
        {
            leaderDisplay.gameObject.SetActive(leader);
            leaderDisplay.color = hero != null && hero.team != null ? 
                hero.team.teamColor : Color.clear;
        }

        foreach (var item in subButtons)
            item.interactable = lastInput != null;
    }
    public void PassHero()
    {
        LastIndex = new(x, y); 
        UI_FullArmyDisplay.SetNewCurrentHero(lastInput);
    }
    public void RemoveHero()
    {
        Unit unit = UI_FullArmyDisplay.CurrentUnit;
        unit.RemoveHero(lastInput);

        UI_FullArmyDisplay.OnUpdateUnit?.Invoke(unit);
    }
    public void SetLeader()
    {
        Unit unit = UI_FullArmyDisplay.CurrentUnit;
        unit.SetAsLeasder(lastInput);

        UI_FullArmyDisplay.OnUpdateUnit?.Invoke(unit);
    }
    public void ExchangeHero()
    {
        Debug.Log($"ADD HERO: attempting to add a new hero {(lastInput != null ? lastInput : "null")}");
        UI_FullArmyDisplay.ExchangeHero(lastInput);
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        //Debug.Log($"<color=Cyan>ENTER</color> : {lastInput}");
        OnHoverPreviewEnter?.Invoke(lastInput);
    } 
    public void OnPointerExit(PointerEventData eventData)
    {
        //Debug.Log("<color=red>EXIT</color>");
        OnHoverPreviewExit?.Invoke();
    }

    public Hero GetLastInput()
    {
        return lastInput;
    }

    public static bool LastIndexMatches(UI_UnitSlot unit)
    {
        bool x = unit.x == LastIndex.x;
        bool y = unit.y == LastIndex.y;

        return x && y;  
    }
}
